using System;

namespace GameLogic
{
    public class RandomUtils
    {
        private static Random _random;

        public static void InitRandom(int seed)
        {
            _random=new Random(seed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">下限</param>
        /// <param name="max">上限</param>
        /// <returns></returns>
        public static int RandomInt(int min, int max)
        {
            return _random.Next(min, max+1);
        }
    }
}