namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Handler for creating new windows. (See Browser.NewWindowAction.NewBrowser)
/// 
/// When the browser needs to open a new window, myHandler.CreateBrowser will be called. Create a 
/// new browser how and where you will, then return it. The new Browser will be filled with
/// the new page.
/// 
/// Call browser.SetNewWindowHandler(NewWindowAction.NewBrowser, myClass) to have a browser use it.
/// </summary>
public interface INewWindowHandler {

	/**
	 * Creates a new Browser object to hold a new page.
	 * The returned Browser object will then be linked and load the new page.
	 */
	Browser CreateBrowser(Browser parent);
}

}
