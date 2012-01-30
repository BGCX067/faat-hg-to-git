namespace Faat.Storage
{
	public interface IStorage
	{
		IPage GetPage(string identity);
		string GetPageData(string identity);
		void SetPageData(string identity, string pageData);
	}
}
