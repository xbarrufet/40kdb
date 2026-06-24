namespace _40kdb.Models;

public enum MiniatureState
{
    Sprue,
    Built,
    Primed,
    Painted
}

public class Miniature
{
    public int MiniatureId { get; set; }
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
    public MiniatureState State { get; set; } = MiniatureState.Sprue;
    public string Edition { get; set; } = "";
    public bool BasePainted { get; set; } = false;
    public bool BaseMagnetized { get; set; } = false;
    public bool Original { get; set; } = true;
    public bool Proxy { get; set; } = false;
    public bool DecalsApplied { get; set; } = false;
}
