<Window x:Class="$rootnamespace$.HelixWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:h="clr-namespace:HelixToolkit;assembly=HelixToolkit" Title="Helix 3D Toolkit" Height="240" Width="320">
    <Grid>
        <h:HelixView3D ZoomToFitWhenLoaded="True">
            <h:DefaultLightsVisual3D/>
            <h:GridLinesVisual3D/>
            <h:BoxVisual3D Fill="Blue" Center="0,0,5" Width="10" Height="10" Length="10"/>
        </h:HelixView3D>
    </Grid>
</Window>
