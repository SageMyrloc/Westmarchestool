namespace Westmarchestool.Core.Entities.HexMap
{
    public enum HexStatus
    {
        Unexplored = 0,  // Not yet discovered
        Verified = 1,    // Single source, believed accurate
        Disputed = 2     // Multiple conflicting reports
    }
}