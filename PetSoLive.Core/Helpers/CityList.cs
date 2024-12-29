public static class CityList
{
    public static List<string> Cities = new List<string>
    {
        "Adana",
        "Adıyaman",
        "Afyonkarahisar",
        "Ağrı",
        "Amasya",
        "Ankara",
        "Antalya",
        "Artvin",
        "Aydın",
        "Balıkesir",
        "Bilecik",
        "Bingöl",
        "Bitlis",
        "Bolu",
        "Burdur",
        "Bursa",
        "Çanakkale",
        "Çankırı",
        "Çorum",
        "Denizli",
        "Diyarbakır",
        "Edirne",
        "Elazığ",
        "Erzincan",
        "Erzurum",
        "Eskişehir",
        "Gaziantep",
        "Giresun",
        "Gümüşhane",
        "Hakkari",
        "Hatay",
        "Isparta",
        "Mersin",
        "İstanbul",
        "İzmir",
        "Kars",
        "Kastamonu",
        "Kayseri",
        "Kırklareli",
        "Kırşehir",
        "Kocaeli",
        "Konya",
        "Kütahya",
    };

    public static List<string> GetDistrictsByCity(string city)
    {
        // Örnek ilçeler, her şehre göre farklı ilçeler eklenebilir
        if (city == "İstanbul")
            return new List<string> { "Kadıköy", "Beşiktaş", "Üsküdar" };
        else if (city == "Ankara")
            return new List<string> { "Çankaya", "Keçiören", "Mamak" };
        else if (city == "Manisa")
            return new List<string> { "Akhisar", "Turgutlu", "Salihli" };
        else if (city == "İzmir")
            return new List<string> { "Karşıyaka", "Konak", "Bornova" };
        else if (city == "Antalya")
            return new List<string> { "Muratpaşa", "Kepez", "Konyaaltı" };
        else if (city == "Trabzon")
            return new List<string> { "Ortahisar", "Akçaabat", "Araklı" };
        else if (city == "Samsun")
            return new List<string> { "İlkadım", "Atakum", "Canik" };
        else if (city == "Eskişehir")
            return new List<string> { "Tepebaşı", "Odunpazarı", "Çamlıca" };
        else if (city == "Diyarbakır")
            return new List<string> { "Bağlar", "Kayapınar", "Yenişehir" };
        else if (city == "Mersin")
            return new List<string> { "Yenişehir", "Toroslar", "Akdeniz" };
        else if (city == "Malatya")
            return new List<string> { "Yeşilyurt", "Battalgazi", "Doğanşehir" };
        else if (city == "Gaziantep")
            return new List<string> { "Şahinbey", "Şehitkamil", "Nizip" };
        else if (city == "Şanlıurfa")
            return new List<string> { "Haliliye", "Eyyübiye", "Karaköprü" };
        else if (city == "Kahramanmaraş")
            return new List<string> { "Dulkadiroğlu", "Onikişubat", "Elbistan" };
        else if (city == "Van")
            return new List<string> { "İpekyolu", "Edremit", "Tuşba" };
        else if (city == "Denizli")
            return new List<string> { "Merkezefe", "Pamukkale", "Çivril" };
        else if (city == "Balıkesir")
            return new List<string> { "Altıeylül", "Karesi", "Bandırma" };
        else if (city == "Aydın")
            return new List<string> { "Merkezefendi", "Efeler", "Söke" };
        else if (city == "Muğla")
            return new List<string> { "Menteşe", "Bodrum", "Fethiye" };
        else if (city == "Kocaeli")
            return new List<string> { "İzmit", "Gebze", "Darıca" };
        else if (city == "Sakarya")
            return new List<string> { "Serdivan", "Adapazarı", "Arifiye" };
        else if (city == "Tekirdağ")
            return new List<string> { "Çorlu", "Süleymanpaşa", "Kapaklı" };
        else if (city == "Edirne")
            return new List<string> { "Merkez", "Keşan", "Uzunköprü" };
        else if (city == "Kırklareli")
            return new List<string> { "Merkez", "Lüleburgaz", "Babaeski" };
        else if (city == "Çanakkale")
            return new List<string> { "Merkez", "Biga", "Çan" };
        else if (city == "Kütahya")
            return new List<string> { "Merkez", "Tavşanlı", "Simav" };
        else if (city == "Afyonkarahisar")
            return new List<string> { "Merkez", "Sandıklı", "Emirdağ" };
        return new List<string>();
    }
}