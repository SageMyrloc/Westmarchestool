using Westmarchestool.Core.Entities.HexMap;
using Westmarchestool.HexMap.Coordinates;

namespace Westmarchestool.HexMap.Generation
{
    /// <summary>
    /// Generates terrain for hexes using procedural generation with neighbor influence.
    /// </summary>
    public class TerrainGenerator
    {
        private readonly Random _random;

        public TerrainGenerator(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// Generate terrain for a hex based on its neighbors.
        /// Uses weighted randomness influenced by surrounding terrain types.
        /// </summary>
        public TerrainType GenerateTerrain(HexCoordinate coordinate, Dictionary<HexCoordinate, TerrainType> existingHexes)
        {
            // Get neighboring hexes
            var neighbors = HexMath.GetNeighbors(coordinate)
                .Where(n => existingHexes.ContainsKey(n))
                .Select(n => existingHexes[n])
                .ToList();

            // If no neighbors, use base weights
            if (!neighbors.Any())
            {
                return GenerateBaseTerrainType();
            }

            // Calculate weighted terrain based on neighbors
            var terrainWeights = CalculateTerrainWeights(neighbors);

            // Select terrain based on weights
            return SelectTerrainByWeight(terrainWeights);
        }

        /// <summary>
        /// Generate base terrain with default weights (no neighbor influence).
        /// </summary>
        private TerrainType GenerateBaseTerrainType()
        {
            var weights = new Dictionary<TerrainType, int>
            {
                { TerrainType.Plains, 30 },
                { TerrainType.Forest, 25 },
                { TerrainType.Hills, 20 },
                { TerrainType.Mountain, 10 },
                { TerrainType.Swamp, 5 },
                { TerrainType.Desert, 5 },
                { TerrainType.Water, 3 },
                { TerrainType.DeepWater, 1 },
                { TerrainType.Tundra, 1 }
            };

            return SelectTerrainByWeight(weights);
        }

        /// <summary>
        /// Calculate terrain weights based on neighboring terrain types.
        /// Similar terrain types cluster together.
        /// </summary>
        private Dictionary<TerrainType, int> CalculateTerrainWeights(List<TerrainType> neighbors)
        {
            var weights = new Dictionary<TerrainType, int>
            {
                { TerrainType.Plains, 10 },
                { TerrainType.Forest, 10 },
                { TerrainType.Hills, 10 },
                { TerrainType.Mountain, 10 },
                { TerrainType.Swamp, 5 },
                { TerrainType.Desert, 5 },
                { TerrainType.Water, 5 },
                { TerrainType.DeepWater, 2 },
                { TerrainType.Tundra, 2 },
                { TerrainType.Jungle, 2 }
            };

            // Increase weight for terrain types present in neighbors
            foreach (var neighborTerrain in neighbors)
            {
                if (weights.ContainsKey(neighborTerrain))
                {
                    weights[neighborTerrain] += 40; // Strong neighbor influence
                }

                // Apply terrain affinity rules
                ApplyTerrainAffinity(weights, neighborTerrain);
            }

            return weights;
        }

        /// <summary>
        /// Apply terrain affinity rules - certain terrains make others more likely.
        /// </summary>
        private void ApplyTerrainAffinity(Dictionary<TerrainType, int> weights, TerrainType neighborTerrain)
        {
            switch (neighborTerrain)
            {
                case TerrainType.Forest:
                    weights[TerrainType.Plains] += 15;
                    weights[TerrainType.Hills] += 10;
                    weights[TerrainType.Swamp] += 5;
                    break;

                case TerrainType.Mountain:
                    weights[TerrainType.Hills] += 20;
                    weights[TerrainType.Tundra] += 10;
                    break;

                case TerrainType.Hills:
                    weights[TerrainType.Plains] += 10;
                    weights[TerrainType.Forest] += 10;
                    weights[TerrainType.Mountain] += 15;
                    break;

                case TerrainType.Swamp:
                    weights[TerrainType.Water] += 15;
                    weights[TerrainType.Forest] += 10;
                    break;

                case TerrainType.Water:
                    weights[TerrainType.DeepWater] += 20;
                    weights[TerrainType.Swamp] += 10;
                    weights[TerrainType.Plains] += 5;
                    break;

                case TerrainType.DeepWater:
                    weights[TerrainType.Water] += 25;
                    break;

                case TerrainType.Desert:
                    weights[TerrainType.Plains] += 10;
                    weights[TerrainType.Hills] += 5;
                    break;

                case TerrainType.Tundra:
                    weights[TerrainType.Mountain] += 15;
                    weights[TerrainType.Hills] += 10;
                    break;

                case TerrainType.Jungle:
                    weights[TerrainType.Forest] += 15;
                    weights[TerrainType.Swamp] += 10;
                    break;
            }
        }

        /// <summary>
        /// Select a terrain type based on weighted randomness.
        /// </summary>
        private TerrainType SelectTerrainByWeight(Dictionary<TerrainType, int> weights)
        {
            var totalWeight = weights.Values.Sum();
            var randomValue = _random.Next(totalWeight);

            var cumulativeWeight = 0;
            foreach (var kvp in weights)
            {
                cumulativeWeight += kvp.Value;
                if (randomValue < cumulativeWeight)
                {
                    return kvp.Key;
                }
            }

            // Fallback (should never reach here)
            return TerrainType.Plains;
        }
    }
}