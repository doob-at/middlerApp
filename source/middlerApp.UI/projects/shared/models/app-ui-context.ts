import { TemplateRef } from '@angular/core';

export class AppUIContext {
    Header = new AppUIHeader();
    Footer = new AppUIFooter();
    Content = new AppUIContent();

    Sidebar = new AppUISideBar();

    Main = new AppUIMain();
}

export class AppUIHeader {

    Title: string | null = null;
    SubTitle: string | null = null;
    Icon: string = "";

    Outlet: TemplateRef<any> | null = null;
}

class AppUIFooter {

    Show: boolean = true;

    Button1 = new AppUIButton();
    Button2 = new AppUIButton();
    Button3 = new AppUIButton();

    Outlet: TemplateRef<any> | null = null;
    UseTemplate: TemplateRef<any> | null = null;
}

class AppUIButton {

    public Text: string = "";
    public Disabled: boolean = false;
    public Loading: boolean = false;
    public Visible: boolean = false;

    public OnClick = () => {};
    // public OnClick(action: () => void) {
    //     action()
    // }
}

class AppUIContent {
    Scrollable: boolean = false;
    Container: boolean = false;

    ShowAlways: boolean = false;
}

class AppUISideBar {

    Hide: boolean = false;


}

class AppUIMain {
    Hide: boolean = false
}