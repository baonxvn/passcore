export interface IChangePasswordFormInitialModel {
    CurrentPassword: string;
    NewPassword: string;
    NewPasswordVerify: string;
    ReCaptcha: string;
    Username: string;
}

export interface IChangePasswordFormProps {
    submitData: boolean;
    toSubmitData: any;
    parentRef: any;
    onValidated: any;
    shouldReset: boolean;
    changeResetState: any;
    setReCaptchaToken: any;
    reCaptchaToken: string;
}

export interface IPasswordGenProps {
    value: string;
    setValue: any;
}
