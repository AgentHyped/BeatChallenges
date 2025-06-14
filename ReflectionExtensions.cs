using System.Reflection;

public static class ReflectionExtensions
{
    public static T GetPrivateField<T>(this object obj, string fieldName)
    {
        var field = obj.GetType()
                       .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            throw new Exception($"Field '{fieldName}' not found in {obj.GetType().Name}");

        return (T)field.GetValue(obj);
    }

    public static void SetPrivateField<T>(this object obj, string fieldName, T value)
    {
        var field = obj.GetType()
                       .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            throw new Exception($"Field '{fieldName}' not found in {obj.GetType().Name}");

        field.SetValue(obj, value);
    }
}