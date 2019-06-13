public class RendererObject
{
    public int instanceId;
    public string sortingLayerName;
    public int sortingOrder;
    public float sortingOrder2 = -999.99f;

    public RendererObject(int instanceId, string layerName, int sortOrder)
    {
        this.instanceId = instanceId;
        sortingLayerName = layerName;
        sortingOrder = sortOrder;
    }
    

    public RendererObject(int instanceId, string layerName, float sortOrder)
    {
        this.instanceId = instanceId;
        sortingLayerName = layerName;
        sortingOrder2 = sortOrder;
    }
}