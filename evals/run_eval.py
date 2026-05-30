import argparse
import csv
import json
import time
from pathlib import Path

import requests

def load_jsonl(path: Path) -> list[dict]:
    rows = []
    with path.open("r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line:
                rows.append(json.loads(line))
    return rows

def contains_expected(answer: str, expected_items: list[str]) -> bool:
    if not expected_items:
        return False
    
    answer_lower = answer.lower()
    
    return any(
        expected.lower() in answer_lower
        for expected in expected_items
    )
    
def ask(api_url: str, question: str) -> tuple[str, int, int | None, str | None]:
    started = time.perf_counter()
    
    try:
        response = requests.post(
            api_url,
            json={"question": question},
            timeout=120
        )
        
        latency_ms = int((time.perf_counter() - started) * 1000)
        
        if not response.ok:
            return "", latency_ms, response.status_code, response.text
        
        data = response.json()
        return data.get("answer", ""), latency_ms, response.status_code, None

    except Exception as ex:
        latency_ms = int((time.perf_counter() - started) * 1000)
        return "", latency_ms, None, str(ex)

def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--dataset",
        default="evals/datasets/rag_eval_v1.jsonl",
        help="Path to JSONL eval dataset."
    )
    parser.add_argument(
        "--experiment",
        required=True,
        help="Experiment name, e.g. baseline_no_reranker or local_onnx_reranker.",
    )
    parser.add_argument(
        "--api-url",
        default="http://localhost:5292/ask",
        help="Ask endpoint URL.",
    )
    parser.add_argument(
        "--output",
        default=None,
        help="Output CSV path. Defaults to evals/results/<experiment>.csv.",
    )
    args = parser.parse_args()
    
    dataset_path = Path(args.dataset)
    output_path = Path(args.output or f"evals/results/{args.experiment}.csv")
    output_path.parent.mkdir(parents=True, exist_ok=True)
    
    questions = load_jsonl(dataset_path)
    
    fieldnames = [
        "question_id",
        "experiment",
        "question",
        "answer",
        "expected_answer_contains",
        "contains_expected",
        "latency_ms",
        "status_code",
        "error",
        "notes",
    ]
    
    with output_path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()

        for item in questions:
            question_id = item["id"]
            question = item["question"]
            expected_items = item.get("expected_answer_contains") or []

            print(f"[{args.experiment}] {question_id}: {question}")

            answer, latency_ms, status_code, error = ask(args.api_url, question)
            is_match = contains_expected(answer, expected_items)

            writer.writerow(
                {
                    "question_id": question_id,
                    "experiment": args.experiment,
                    "question": question,
                    "answer": answer,
                    "expected_answer_contains": "; ".join(expected_items),
                    "contains_expected": is_match,
                    "latency_ms": latency_ms,
                    "status_code": status_code,
                    "error": error or "",
                    "notes": "",
                }
            )

            print(f"  status={status_code} latency_ms={latency_ms} contains_expected={is_match}")

    print(f"\nSaved results to: {output_path}")

if __name__ == "__main__":
    main()