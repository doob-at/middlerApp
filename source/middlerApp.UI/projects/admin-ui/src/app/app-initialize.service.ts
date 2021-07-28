import { Injectable } from "@angular/core";
import { AuthService } from "./shared/services";



@Injectable({
    providedIn: 'root'
})
export class AppInitializeService {


    constructor(
        private authService: AuthService
    ) {

       
    }
}