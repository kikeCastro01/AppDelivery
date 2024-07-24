using static Android.Icu.Text.CaseMap;

namespace Notificacion;

public partial class interfazRestaurante : FlyoutPage
{
	public interfazRestaurante()
	{
		InitializeComponent();
		Title = "interfazRestaurante";
		// Suscribirse al evento de selección del menú lateral
		flyoutPage.collectionView.SelectionChanged += CollectionView_SelectionChanged;
	}

	private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var item = e.CurrentSelection.FirstOrDefault() as flyoutPageItem;
		if (item != null)
		{
			Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
			//IsPresented = false;
		}
	}
}