using System;

[Serializable]
public class FrontendData
{
    public MapData map_data;
    public string say;
    
    [Serializable]
    public class MapData
    {
        public string name;
        public string[] image_links;
        public string[] reviews;
    }
}
