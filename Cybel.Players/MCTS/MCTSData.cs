using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Players.MCTS
{
    public class MCTSData
    {
        public struct MoveData
        {
            public double Score { get; set; } = 0;
            public int Visits { get; set; } = 0;

            public MoveData() 
            {
            }
        }

        public List<MoveData> Datas { get; } = new();
        public int TotalVisits => Datas.Sum(x => x.Visits);

        public void Initialize(int moves)
        {
            Datas.Clear();

            for (int i = 0; i < moves; i++)
            {
                Datas.Add(new());
            }
        }

        public int Choose(double c)
        {
            var total = 0;

            for (int i = 0; i < Datas.Count; i++)
            {
                var data = Datas[i];

                if (data.Visits <= 0)
                {
                    return i;
                }

                total += data.Visits;
            }

            var best = 0;
            var best_score = double.MinValue;

            for (int i = 0; i < Datas.Count; i++)
            {
                var data = Datas[i];
                var score = (data.Score / data.Visits) + (c * Math.Sqrt(Math.Log(total) / data.Visits));

                if (score > best_score)
                {
                    best = i;
                    best_score = score;
                }
            }

            return best;
        }
    }
}
