public class ToggleButtonColorUI : ToggleButtonUIBase
{
    private ToggleColor toggleColor;
    protected override void Awake()
    {
        base.Awake();
        toggleColor = GetComponent<ToggleColor>();
    }

    public override void ChangeState(State newState)
    {
        base.ChangeState(newState);
        toggleColor.Toggle(newState == State.Active);
    }
}