namespace AiKnowledgeAssistant.Utils
{
    public static class TextChunking
    {
        public static List<string> ChunkText(string text, int chunkSize = 1000)
        {
            var chunks = new List<string>();

            for (int i = 0; i < text.Length; i+= chunkSize)
            {
                chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
            }
            return chunks;
        }
    }
}
