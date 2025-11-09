namespace Westmarchestool.Core.Entities.HexMap
{
    public enum ExpeditionStatus
    {
        Active = 0,      // Currently exploring
        Completed = 1,   // Ended and merged to Town Map
        Archived = 2     // Historical record, read-only
    }
}