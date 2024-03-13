using Cybel.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Cybel.Learning
{
    public abstract class Optimizer(Func<LearningPlayer> player_factory)
    {
        protected IReadOnlyList<Parameter> Parameters { get; } = player_factory().GetParameters().ToList();

        private Func<LearningPlayer> PlayerFactory { get; } = player_factory;

        public abstract Solution Step(IGame game);
        public abstract void Load(Stream stream);
        public abstract void Save(Stream stream);

        protected void SetFitness(IEnumerable<Solution> solutions, IEnumerable<Solution> tests, IGame game, int games)
        {
            SetFitness(solutions, tests.Select(GetPlayer).ToList(), game, games);
        }

        protected void SetFitness(IEnumerable<Solution> solutions, IEnumerable<Player> tests, IGame game, int games)
        {
            foreach (var solution in solutions)
            {
                solution.Fitness = 0;
            }

            foreach (var test in tests)
            {
                var runner = new Runner();
                var results = new Dictionary<Solution, double>();
                var total = 1e-6;

                foreach (var solution in solutions)
                {
                    var player = GetPlayer(solution);
                    var score = runner.Play(game, [player, test], games)[player];
                    total += score;
                    results.Add(solution, score);
                }

                foreach (var solution in solutions)
                {
                    lock (solution)
                    {
                        solution.Fitness += results[solution] / total;
                    }
                }
            }
        }

        protected void Randomize(Solution solution)
        {
            foreach (var parameter in Parameters)
            {
                var value = parameter.Min + Random.Shared.NextDouble() * (parameter.Max - parameter.Min);
                solution.Parameters[parameter.Name] = value;
            }
        }

        private LearningPlayer GetPlayer(Solution solution)
        {
            var player = PlayerFactory();

            foreach (var parameter in Parameters)
            {
                player.SetParameter(parameter.Name, solution.Parameters[parameter.Name]);
            }

            return player;
        }
    }
}
