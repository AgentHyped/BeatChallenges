namespace BeatChallenges
{
    public class PluginConfig
    {
        public virtual bool Enabled { get; set; } = true;

        public virtual void OnReload() { }

        public virtual void Changed() { }

        public virtual void CopyFrom(PluginConfig other) { }
    }
}