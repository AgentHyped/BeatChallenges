namespace BeatChallenges.Challenges
{
    public interface IChallenge
    {
        string Name { get; }
        void StartChallenge();
        void StopChallenge();
        void PauseChallenge();
        void ResumeChallenge();
        bool IsCompleted { get; }
        bool HasFailed { get; }
        Action OnCompleted { get; set; }
    }
}