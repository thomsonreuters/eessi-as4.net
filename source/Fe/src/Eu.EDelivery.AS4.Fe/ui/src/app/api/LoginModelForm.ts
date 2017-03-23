import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoginModel } from './LoginModel';

export class LoginModelForm {
    public static getForm(formBuilder: FormBuilder, current: LoginModel): FormGroup {
        return formBuilder.group({
            username: [current && current.username],
            password: [current && current.password],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: LoginModel) {
        form.removeControl(LoginModel.FIELD_username);
        form.addControl(LoginModel.FIELD_username, formBuilder.control(current && current.username));
        form.removeControl(LoginModel.FIELD_password);
        form.addControl(LoginModel.FIELD_password, formBuilder.control(current && current.password));
    }
}
