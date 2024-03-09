using Cybel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Cybel.Learning
{
    public abstract class Optimizer<TPlayer> where TPlayer : LearningPlayer, new()
    {
        public abstract Dictionary<Parameter, double> Step(IGame game);
        public abstract void Load(Stream stream);
        public abstract void Save(Stream stream);

        protected void SetFitness(IEnumerable<Solution> solutions, IEnumerable<Solution> tests, IGame game, int games)
        {
            SetFitness(solutions, tests.Select(GetPlayer), game, games);
        }

        protected void SetFitness(IEnumerable<Solution> solutions, IEnumerable<Player> tests, IGame game, int games)
        {
            var runner = new Runner();
            var results = new Dictionary<(Solution, Player), double>();

            foreach (var test in tests)
            {
                foreach (var solution in solutions)
                {
                    var player = GetPlayer(solution);
                    var score = runner.Play(game, [player, test], games)[player];
                    results.Add((solution, test), score);
                    solution.Fitness = 0;
                }
            }

            foreach (var test in tests)
            {
                var total = Math.Max(results.Sum(x => x.Key.Item2 == test ? x.Value : 0), 1e-6);

                foreach (var solution in solutions)
                {
                    var score = results[(solution, test)];
                    solution.Fitness += score / total;
                }
            }
        }

        protected IReadOnlyList<Parameter> GetParameters()
        {
            var player = new TPlayer();

            return player.GetParameters().ToList();
        }

        protected void Randomize(Solution solution)
        {
            var rng = new Random(Guid.NewGuid().GetHashCode());
            var parameters = GetParameters();

            foreach (var parameter in parameters)
            {
                var value = parameter.Min + rng.NextDouble() * (parameter.Max - parameter.Min);
                solution.Parameters[parameter.Name] = value;
            }
        }

        private TPlayer GetPlayer(Solution solution)
        {
            var player = new TPlayer();

            foreach (var parameter in player.GetParameters())
            {
                player.SetParameter(parameter.Name, solution.Parameters[parameter.Name]);
            }

            return player;
        }
    }
}
