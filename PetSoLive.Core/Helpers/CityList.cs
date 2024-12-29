public static class CityList
{
    public static List<string> Cities = new List<string>
    {
        "Manisa",
        "İzmir",
        "İstanbul",
        "Muğla",
    };

    public static List<string> GetDistrictsByCity(string city)
    {
        // Örnek ilçeler, her şehre göre farklı ilçeler eklenebilir
        if (city == "İstanbul")
            return new List<string> { "Kadıköy", "Beşiktaş", "Üsküdar" };
        else if (city == "Manisa")
            return new List<string> { "Akhisar", "Turgutlu", "Salihli" };
        else if (city == "İzmir")
            return new List<string> { "Karşıyaka", "Konak", "Bornova" };
        else if (city == "Muğla")
            return new List<string> { "Menteşe", "Bodrum", "Fethiye","Milas" };
        return new List<string>();
    }
}