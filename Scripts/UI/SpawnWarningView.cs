using Godot;

public partial class SpawnWarningView : Control
{
    [Export(PropertyHint.Range, "0.1,3,0.05")]
    public float DefaultWarningDurationSeconds { get; set; } = 0.75f;

    [Export(PropertyHint.Range, "8,128,1")]
    public float EdgeMarginPixels { get; set; } = 48f;

    private Label _indicator;
    private double _remainingSeconds;

    public override void _Ready()
    {
        _indicator = GetNode<Label>("Indicator");
        _indicator.MouseFilter = Control.MouseFilterEnum.Ignore;
        Hide();
    }

    public override void _Process(double delta)
    {
        if (_remainingSeconds <= 0)
        {
            return;
        }

        _remainingSeconds -= delta;
        if (_remainingSeconds <= 0)
        {
            _remainingSeconds = 0;
            Hide();
        }
    }

    public void ShowWarning(Vector2 screenDirection, float durationSeconds = -1f)
    {
        if (_indicator == null || !IsFinite(screenDirection) || screenDirection.LengthSquared() <= 0.0001f)
        {
            return;
        }

        Vector2 direction = screenDirection.Normalized();
        Vector2 center = Size * 0.5f;
        Vector2 availableHalfSize = new(
            Mathf.Max(0, center.X - EdgeMarginPixels),
            Mathf.Max(0, center.Y - EdgeMarginPixels));

        if (availableHalfSize.X <= 0 || availableHalfSize.Y <= 0)
        {
            return;
        }

        float scaleX = Mathf.Abs(direction.X) > 0.0001f
            ? availableHalfSize.X / Mathf.Abs(direction.X)
            : float.PositiveInfinity;
        float scaleY = Mathf.Abs(direction.Y) > 0.0001f
            ? availableHalfSize.Y / Mathf.Abs(direction.Y)
            : float.PositiveInfinity;
        float scale = Mathf.Min(scaleX, scaleY);

        Vector2 edgeCenter = center + direction * scale;
        _indicator.Position = edgeCenter - _indicator.Size * 0.5f;
        _indicator.Rotation = Mathf.Atan2(direction.Y, direction.X) + Mathf.Pi * 0.5f;

        _remainingSeconds = durationSeconds > 0 ? durationSeconds : DefaultWarningDurationSeconds;
        Show();
    }

    private static bool IsFinite(Vector2 value) => float.IsFinite(value.X) && float.IsFinite(value.Y);
}
