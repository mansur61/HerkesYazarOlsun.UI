namespace HerkesYazarOlsun.Portal.Helpers;
public static class BookPagination
{
    public const int ExtraPages = 4; // front cover, preface, closing page, back cover
    public static int Total(int contentPages) => contentPages + ExtraPages;
    public static int Next(int contentPages) => Total(contentPages) + 1;
}
