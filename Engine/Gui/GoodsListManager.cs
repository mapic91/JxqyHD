namespace Engine.Gui
{
    public static class GoodsListManager
    {
        private const int MaxGoods = 223;

        public class GoodsItemInfo
        {
            public Good TheGood;
            public int Count;

            public GoodsItemInfo(Good good, int count)
            {
                TheGood = good;
                Count = count;
            }
        }
    }
}