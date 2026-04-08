namespace RentalClient.Interface;

public enum PageType
{
    Main,
    SignIn,
    SignUp
    
}

public enum WorkplaceType
{
    Search,
    Profile,
    CreateListing
}

public interface IViewInterface
{
    event EventHandler<PageType> NavigationPageRequested; 
}

public interface IMainViewInterface
{
    event EventHandler<WorkplaceType> NavigationWorkplaceRequested;
}