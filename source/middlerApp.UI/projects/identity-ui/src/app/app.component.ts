import { Component } from '@angular/core';
import { AppUIService } from '@shared/services';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(
    //private initService: AppInitializeService, 
    private uiService: AppUIService,
    public oidcSecurityService: OidcSecurityService) {

    uiService.SetDefault(ui => {
      ui.Content.Scrollable = false;
      ui.Content.Container = true;
      ui.Header.Icon = "";
      ui.Footer.Show = false;
    })


  }

  // ngOnInit() {
  //   this.oidcSecurityService.checkAuth().subscribe((resp) => {
  //     resp;
  //   });
  // }
}
