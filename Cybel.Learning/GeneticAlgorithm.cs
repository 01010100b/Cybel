using Cybel.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Cybel.Learning
{
    public class GeneticAlgorithm : Optimizer
    {
        private double MutateChance { get; } = 0.5;
        private int Games { get; } = 10;
        private List<Solution> Solutions { get; } = [];
        private List<Solution> Tests { get; } = [];
        private List<Solution> HallOfFame { get; } = [];

        public GeneticAlgorithm(int population, int games, double mutate_chance, Func<LearningPlayer> player_factory) : base(player_factory)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(population, 4);
            ArgumentOutOfRangeException.ThrowIfLessThan(games, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(mutate_chance, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(mutate_chance, 1);

            Games = games;
            MutateChance = mutate_chance;

            for (int i = 0; i < population / 2; i++)
            {
                Solutions.Add(new());
                Tests.Add(new());
                Randomize(Solutions[i]);
                Randomize(Tests[i]);
            }
        }

        public Solution GetBest()
        {
            Debug.Assert(HallOfFame.Count > 0);

            return HallOfFame[^1];
        }

        public override Solution Step(IGame game)
        {
            var tests = Tests.ToList();

            for (int i = 0; i < Math.Min(Tests.Count, HallOfFame.Count); i++)
            {
                tests.Add(HallOfFame[^(i + 1)]);
            }

            SetFitness(Solutions, tests, game, Games);

            Solution? best = null;
            double best_fitness = double.MinValue;

            foreach (var solution in Solutions)
            {
                if (solution.Fitness > best_fitness)
                {
                    best = solution;
                    best_fitness = solution.Fitness;
                }
            }

            Debug.Assert(best is not null);

            lock (Solutions)
            {
                var solutions = Breed(Solutions);
                Solutions.Clear();
                Solutions.AddRange(Tests);
                Tests.Clear();
                Tests.AddRange(solutions);
                HallOfFame.Add(best);
            }

            return best;
        }

        public override void Load(Stream stream)
        {
            var state = JsonSerializer.Deserialize<Tuple<List<Solution>, List<Solution>, List<Solution>>>(stream)!;
            Solutions.Clear();
            Tests.Clear();
            HallOfFame.Clear();
            Solutions.AddRange(state.Item1);
            Tests.AddRange(state.Item2);
            HallOfFame.AddRange(state.Item3);
        }

        public override void Save(Stream stream)
        {
            lock (Solutions)
            {
                var state = Tuple.Create(Solutions, Tests, HallOfFame);
                var options = new JsonSerializerOptions() { WriteIndented = true };
                JsonSerializer.Serialize(stream, state, options);
                stream.Flush();
            }
        }

        private List<Solution> Breed(List<Solution> solutions)
        {
            var result = new List<Solution>();

            solutions.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
            result.Add(solutions[0]);

            while (result.Count < solutions.Count)
            {
                var parent1 = Random.Shared.Choose(solutions.Select(x => new KeyValuePair<Solution, double>(x, x.Fitness)));
                var parent2 = Random.Shared.Choose(solutions.Select(x => new KeyValuePair<Solution, double>(x, x.Fitness)));
                
                while (parent2 == parent1)
                {
                    parent2 = Random.Shared.Choose(solutions.Select(x => new KeyValuePair<Solution, double>(x, x.Fitness)));
                }
                
                var child = Crossover(parent1, parent2);
                Mutate(child, MutateChance);
                result.Add(child);
            }

            return result;
        }

        private Solution Crossover(Solution parent1, Solution parent2)
        {
            var solution = new Solution();

            foreach (var name in Parameters.Select(x => x.Name))
            {
                if (Random.Shared.NextDouble() < 0.5)
                {
                    solution.Parameters.Add(name, parent1.Parameters[name]);
                }
                else
                {
                    solution.Parameters.Add(name, parent2.Parameters[name]);
                }
            }

            return solution;
        }

        private void Mutate(Solution solution, double chance)
        {
            foreach (var par in Parameters)
            {
                if (Random.Shared.NextDouble() < chance)
                {
                    var value = par.Min + Random.Shared.NextDouble() * (par.Max - par.Min);
                    solution.Parameters[par.Name] = value;
                }
            }
        }
    }
}
