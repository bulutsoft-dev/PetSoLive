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
        "Malatya",
        "Manisa",
        "Kahramanmaraş",
        "Mardin",
        "Muğla",
        "Muş",
        "Nevşehir",
        "Niğde",
        "Ordu",
        "Rize",
        "Sakarya",
        "Samsun",
        "Siirt",
        "Sinop",
        "Sivas",
        "Tekirdağ",
        "Tokat",
        "Trabzon",
        "Tunceli",
        "Şanlıurfa",
        "Uşak",
        "Van",
        "Yozgat",
        "Zonguldak",
        "Aksaray",
        "Bayburt",
        "Karaman",
        "Kırıkkale",
        "Batman",
        "Şırnak",
        "Bartın",
        "Ardahan",
        "Iğdır",
        "Yalova",
        "Karabük",
        "Kilis",
        "Osmaniye",
        "Düzce"
    };

    public static List<string> GetDistrictsByCity(string city)
    {
        switch (city)
        {
            case "Adana": return new List<string> { "Seyhan", "Yüreğir", "Çukurova", "Ceyhan" };
            case "Adıyaman": return new List<string> { "Merkez", "Besni", "Kahta", "Gölbaşı" };
            case "Afyonkarahisar": return new List<string> { "Merkez", "Sandıklı", "Emirdağ", "Dinar" };
            case "Ağrı": return new List<string> { "Merkez", "Patnos", "Doğubayazıt", "Eleşkirt" };
            case "Amasya": return new List<string> { "Merkez", "Suluova", "Merzifon", "Taşova" };
            case "Ankara": return new List<string> { "Çankaya", "Keçiören", "Mamak", "Etimesgut" };
            case "Antalya": return new List<string> { "Muratpaşa", "Kepez", "Konyaaltı", "Alanya" };
            case "Artvin": return new List<string> { "Merkez", "Hopa", "Arhavi", "Yusufeli" };
            case "Aydın": return new List<string> { "Efeler", "Nazilli", "Söke", "Kuşadası" };
            case "Balıkesir": return new List<string> { "Altıeylül", "Karesi", "Edremit", "Bandırma" };
            case "Bilecik": return new List<string> { "Merkez", "Bozüyük", "Söğüt", "Pazaryeri" };
            case "Bingöl": return new List<string> { "Merkez", "Genç", "Solhan", "Karlıova" };
            case "Bitlis": return new List<string> { "Merkez", "Tatvan", "Ahlat", "Adilcevaz" };
            case "Bolu": return new List<string> { "Merkez", "Gerede", "Mengen", "Mudurnu" };
            case "Burdur": return new List<string> { "Merkez", "Bucak", "Yeşilova", "Gölhisar" };
            case "Bursa": return new List<string> { "Osmangazi", "Yıldırım", "Nilüfer", "İnegöl" };
            case "Çanakkale": return new List<string> { "Merkez", "Biga", "Çan", "Gelibolu" };
            case "Çankırı": return new List<string> { "Merkez", "Çerkeş", "Ilgaz", "Kurşunlu" };
            case "Çorum": return new List<string> { "Merkez", "Sungurlu", "Alaca", "Osmancık" };
            case "Denizli": return new List<string> { "Pamukkale", "Merkezefendi", "Çivril", "Tavas" };
            case "Diyarbakır": return new List<string> { "Bağlar", "Kayapınar", "Yenişehir", "Ergani" };
            case "Edirne": return new List<string> { "Merkez", "Keşan", "Uzunköprü", "Havsa" };
            case "Elazığ": return new List<string> { "Merkez", "Kovancılar", "Karakoçan", "Maden" };
            case "Erzincan": return new List<string> { "Merkez", "Tercan", "Üzümlü", "Çayırlı" };
            case "Erzurum": return new List<string> { "Yakutiye", "Palandöken", "Aziziye", "Horasan" };
            case "Eskişehir": return new List<string> { "Tepebaşı", "Odunpazarı", "Sivrihisar", "Çifteler" };
            case "Gaziantep": return new List<string> { "Şahinbey", "Şehitkamil", "Nizip", "İslahiye" };
            case "Giresun": return new List<string> { "Merkez", "Bulancak", "Görele", "Espiye" };
            case "Gümüşhane": return new List<string> { "Merkez", "Kelkit", "Şiran", "Köse" };
            case "Hakkari": return new List<string> { "Merkez", "Yüksekova", "Çukurca", "Şemdinli" };
            case "Hatay": return new List<string> { "Antakya", "İskenderun", "Dörtyol", "Samandağ" };
            case "Isparta": return new List<string> { "Merkez", "Yalvaç", "Eğirdir", "Şarkikaraağaç" };
            case "Mersin": return new List<string> { "Yenişehir", "Toroslar", "Akdeniz", "Tarsus" };
            case "İstanbul": return new List<string> { "Kadıköy", "Beşiktaş", "Üsküdar", "Fatih" };
            case "İzmir": return new List<string> { "Karşıyaka", "Konak", "Bornova", "Buca" };
            case "Kars": return new List<string> { "Merkez", "Sarıkamış", "Kağızman", "Akyaka" };
            case "Kastamonu": return new List<string> { "Merkez", "Tosya", "Taşköprü", "İnebolu" };
            case "Kayseri": return new List<string> { "Melikgazi", "Kocasinan", "Talas", "Develi" };
            case "Kırklareli": return new List<string> { "Merkez", "Lüleburgaz", "Babaeski", "Vize" };
            case "Kırşehir": return new List<string> { "Merkez", "Kaman", "Mucur", "Çiçekdağı" };
            case "Kocaeli": return new List<string> { "İzmit", "Gebze", "Darıca", "Körfez" };
            case "Konya": return new List<string> { "Selçuklu", "Karatay", "Meram", "Ereğli" };
            case "Kütahya": return new List<string> { "Merkez", "Tavşanlı", "Simav", "Emet" };
            case "Malatya": return new List<string> { "Yeşilyurt", "Battalgazi", "Doğanşehir", "Akçadağ" };
            case "Manisa": return new List<string> { "Akhisar", "Turgutlu", "Salihli", "Alaşehir" };
            case "Kahramanmaraş": return new List<string> { "Dulkadiroğlu", "Onikişubat", "Elbistan", "Afşin" };
            case "Mardin": return new List<string> { "Artuklu", "Midye", "Nusaybin", "Kızıltepe" };
            case "Muğla": return new List<string> { "Menteşe", "Bodrum", "Fethiye", "Marmaris" };
            case "Muş": return new List<string> { "Merkez", "Bulanık", "Malazgirt", "Varto" };
            case "Nevşehir": return new List<string> { "Merkez", "Ürgüp", "Avanos", "Gülşehir" };
            case "Niğde": return new List<string> { "Merkez", "Bor", "Ulukışla", "Çamardı" };
            case "Ordu": return new List<string> { "Altınordu", "Ünye", "Fatsa", "Perşembe" };
            case "Rize": return new List<string> { "Merkez", "Çayeli", "Pazar", "Ardeşen" };
            case "Sakarya": return new List<string> { "Adapazarı", "Serdivan", "Akyazı", "Hendek" };
            case "Samsun": return new List<string> { "İlkadım", "Atakum", "Canik", "Bafra" };
            case "Siirt": return new List<string> { "Merkez", "Kurtalan", "Pervari", "Baykan" };
            case "Sinop": return new List<string> { "Merkez", "Boyabat", "Ayancık", "Durağan" };
            case "Sivas": return new List<string> { "Merkez", "Şarkışla", "Suşehri", "Zara" };
            case "Tekirdağ": return new List<string> { "Çorlu", "Süleymanpaşa", "Malkara", "Çerkezköy" };
            case "Tokat": return new List<string> { "Merkez", "Turhal", "Zile", "Erbaa" };
            case "Trabzon": return new List<string> { "Ortahisar", "Akçaabat", "Araklı", "Vakfıkebir" };
            case "Tunceli": return new List<string> { "Merkez", "Pertek", "Çemişgezek", "Hozat" };
            case "Şanlıurfa": return new List<string> { "Haliliye", "Eyyübiye", "Karaköprü", "Siverek" };
            case "Uşak": return new List<string> { "Merkez", "Banaz", "Sivaslı", "Eşme" };
            case "Van": return new List<string> { "İpekyolu", "Edremit", "Tuşba", "Erciş" };
            case "Yozgat": return new List<string> { "Merkez", "Sorgun", "Akdağmadeni", "Boğazlıyan" };
            case "Zonguldak": return new List<string> { "Merkez", "Ereğli", "Devrek", "Çaycuma" };
            case "Aksaray": return new List<string> { "Merkez", "Sultanhanı", "Eskil", "Ortaköy" };
            case "Bayburt": return new List<string> { "Merkez", "Aydıntepe", "Demirözü", "Diğer" };
            case "Karaman": return new List<string> { "Merkez", "Ermenek", "Ayrancı", "Sarıveliler" };
            case "Kırıkkale": return new List<string> { "Merkez", "Keskin", "Delice", "Sulakyurt" };
            case "Batman": return new List<string> { "Merkez", "Kozluk", "Sason", "Hasankeyf" };
            case "Şırnak": return new List<string> { "Merkez", "Cizre", "Silopi", "İdil" };
            case "Bartın": return new List<string> { "Merkez", "Amasra", "Ulus", "Kurucaşile" };
            case "Ardahan": return new List<string> { "Merkez", "Göle", "Hanak", "Posof" };
            case "Iğdır": return new List<string> { "Merkez", "Tuzluca", "Aralık", "Karakoyunlu" };
            case "Yalova": return new List<string> { "Merkez", "Çınarcık", "Termal", "Altınova" };
            case "Karabük": return new List<string> { "Merkez", "Safranbolu", "Yenice", "Eflani" };
            case "Kilis": return new List<string> { "Merkez", "Musabeyli", "Polateli", "Diğer" };
            case "Osmaniye": return new List<string> { "Merkez", "Kadirli", "Düziçi", "Bahçe" };
            case "Düzce": return new List<string> { "Merkez", "Akçakoca", "Yığılca", "Gölyaka" };
            default: return new List<string> { "Diğer" }; // Fallback for any unexpected cities
        }
    }
}