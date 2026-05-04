namespace RentalClient.Services
{
    public interface ISearchStateService
    {
        string SearchText { get; set; }
        event Action SearchRequested;
        void TriggerSearch();
    }

    public class SearchStateService : ISearchStateService
    {
        public string SearchText { get; set; } = string.Empty;
        public event Action SearchRequested;

        public void TriggerSearch()
        {
            SearchRequested?.Invoke();
        }
    }
    
    public interface IListingStateService
    {
        Guid SelectedListingId { get; set; }
        Guid EditingListingId { get; set; }
    }

    public class ListingStateService : IListingStateService
    {
        public Guid SelectedListingId { get; set; }
        public Guid EditingListingId { get; set; }
    }
}