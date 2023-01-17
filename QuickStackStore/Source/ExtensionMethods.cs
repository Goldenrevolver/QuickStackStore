namespace QuickStackStore
{
    public static class ExtensionMethods
    {
        public static int GridVectorToGridIndex(this ItemDrop.ItemData item, int width)
        {
            return item.m_gridPos.y * width + item.m_gridPos.x;
        }
    }
}
