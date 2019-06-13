//Zenith Code 2014

public class RendererHighlighterConstants
{
    public const string BLANK = "Blank";
	public const string ICONS = "Icons";
	public const string DEFAULT_LAYER_NAME = "Default";
	public static string basePath = "Assets/RenderingOrderHighlighterTool/";
	public static string baseResourcesPath = string.Format("{0}{1}/", basePath, RESOURCES);
	public static string assetFilePath = string.Format("{0}{1}", baseResourcesPath, ASSET_FILE);
    public static string iconsFilePath = string.Format("{0}{1}/", basePath, ICONS);
	public static string defaultIconPath = string.Format("{0}{1}", basePath, DEFAULT_ICON);
    public const string TIF_EXTENSION = ".tif";

	private const string ASSET_FILE = "RenderHighlightersData.asset";
	private const string DEFAULT_ICON = "Icons/Blank.tif";
	private const string RESOURCES = "Resources";
}
