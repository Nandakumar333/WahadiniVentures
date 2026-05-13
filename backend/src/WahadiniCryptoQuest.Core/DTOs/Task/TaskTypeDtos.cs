namespace WahadiniCryptoQuest.Core.DTOs.Task;

public class QuizSubmissionDto
{
    public Dictionary<int, int> Answers { get; set; } = new(); // QuestionIndex -> OptionIndex
}

public class QuizTaskDataDto
{
    public int PassingScore { get; set; } = 80;
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class QuizQuestionDto
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectOption { get; set; }
}

public class TextSubmissionDto
{
    public string Text { get; set; } = string.Empty;
}

public class ExternalLinkSubmissionDto
{
    public string Url { get; set; } = string.Empty;
}

public class WalletSubmissionDto
{
    public string WalletAddress { get; set; } = string.Empty;
    public string? TransactionHash { get; set; }
}
