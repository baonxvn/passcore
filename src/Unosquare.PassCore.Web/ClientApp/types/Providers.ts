interface IAlerts {
    errorCaptcha: string;
    errorComplexPassword: string;
    errorConnectionLdap: string;
    errorFieldMismatch: string;
    errorFieldRequired: string;
    errorInvalidCredentials: string;
    errorInvalidDomain: string;
    errorInvalidUser: string;
    errorPasswordChangeNotAllowed: string;
    errorScorePassword: string;
    errorDistancePassword: string;
    successAlertBody: string;
    successAlertTitle: string;
    errorPwnedPassword: string;
}

interface IChangePasswordForm {
    changePasswordButtonLabel: string;
    currentPasswordHelpBlock: string;
    currentPasswordLabel: string;
    helpText: string;
    newPasswordHelpBlock: string;
    newPasswordLabel: string;
    newPasswordVerifyHelpBlock: string;
    newPasswordVerifyLabel: string;
    usernameDefaultDomainHelperBlock: string;
    usernameHelpBlock: string;
    usernameLabel: string;
}

interface IErrorsPasswordForm {
    fieldRequired: string;
    passwordMatch: string;
    usernameEmailPattern: string;
    usernamePattern: string;
}

interface IReCaptcha {
    siteKey: string;
    privateKey: string;
    languageCode: string;
}

interface IValidationRegex {
    emailRegex: string;
    usernameRegex: string;
}

export interface IGlobalContext {
    alerts: IAlerts;
    applicationTitle: string;
    changePasswordForm: IChangePasswordForm;
    changePasswordTitle: string;
    usePasswordGeneration: boolean;
    errorsPasswordForm: IErrorsPasswordForm;
    reCaptcha: IReCaptcha;
    showPasswordMeter: boolean;
    useEmail: boolean;
    validationRegex: IValidationRegex;
}

export interface ISnackbarContext {
    sendMessage: (messageText: string, messageType?: string) => void;
}
