import { NgModule } from '@angular/core';
import { NZ_ICONS, NzIconModule } from 'ng-zorro-antd/icon';

import { faAddressCard, faAngleDoubleRight, faChevronLeft, faChevronRight, faCube, faCubes, faDesktop, 
    faEllipsisV, faStream, faToggleOff, faToggleOn, faUser, faUsers, faUsersCog, faUserTag, faKey, faShieldAlt } from '@fortawesome/free-solid-svg-icons';
  import { faAddressCard as farAddressCard, faPlayCircle, faPlusSquare, faStopCircle } from '@fortawesome/free-regular-svg-icons';
  import {
    DashboardOutline, FolderOutline, SettingOutline, FieldBinaryOutline, DeleteOutline,
    ContainerOutline, OrderedListOutline, CodeOutline, FormOutline, RollbackOutline, SyncOutline,
    UserAddOutline, LoginOutline, BugOutline, LockOutline, UserOutline, MoreOutline, MenuUnfoldOutline
  } from '@ant-design/icons-angular/icons';


import { FaConfig, FaIconLibrary } from '@fortawesome/angular-fontawesome';

const antdIcons = [
    DashboardOutline,
    FolderOutline,
    SettingOutline,
    FieldBinaryOutline,
    DeleteOutline,
    ContainerOutline, OrderedListOutline, CodeOutline, FormOutline, RollbackOutline,
    SyncOutline, UserAddOutline,
    LoginOutline,
    BugOutline, LockOutline, UserOutline,
    MoreOutline, MenuUnfoldOutline
];

const _faIcons = [
    faStream,
    faUser,
    faUserTag,
    faDesktop,
    faCube,
    faCubes,
    faAddressCard,
    farAddressCard,
    faEllipsisV,
    faToggleOff,
    faToggleOn,
    faPlayCircle,
    faUsersCog,
    faPlusSquare,
    faUsers,
    faAngleDoubleRight,
    faChevronLeft,
    faChevronRight,
    faKey,
    faStopCircle,
    faShieldAlt
]

@NgModule({
    imports: [NzIconModule],
    exports: [NzIconModule],
    providers: [
        { provide: NZ_ICONS, useValue: antdIcons }
    ]
})
export class IconsProviderModule {

    constructor(private library: FaIconLibrary, faConfig: FaConfig) {
        library.addIcons(..._faIcons)
    }

}
