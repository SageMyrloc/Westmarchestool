namespace Westmarchestool.Core.Entities.HexMap
{
    public enum TerrainType
    {
        // Basic terrain types
        Plains = 0,
        Forest = 1,
        Mountain = 2,
        Hills = 3,
        Swamp = 4,
        Desert = 5,
        Water = 6,

        // Special terrain
        DeepWater = 7,
        Tundra = 8,
        Jungle = 9,

        // Unknown/unexplored
        Unknown = 99
    }
}