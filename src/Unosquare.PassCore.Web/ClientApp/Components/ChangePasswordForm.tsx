import FormGroup from '@material-ui/core/FormGroup/FormGroup';
import * as React from 'react';
import { TextValidator } from 'uno-material-ui';
import { useStateForModel } from 'uno-react';
import { GlobalContext } from '../Provider/GlobalContext';
import { IChangePasswordFormInitialModel, IChangePasswordFormProps } from '../types/Components';
import { PasswordGenerator } from './PasswordGenerator';
import { PasswordStrengthBar } from './PasswordStrengthBar';
import { ReCaptcha } from './ReCaptcha';

const defaultState: IChangePasswordFormInitialModel = {
    CurrentPassword: '',
    NewPassword: '',
    NewPasswordVerify: '',
    ReCaptcha: '',
    Username: new URLSearchParams(window.location.search).get('userName') || '',
};

export const ChangePasswordForm: React.FunctionComponent<IChangePasswordFormProps> = ({
    submitData,
    toSubmitData,
    parentRef,
    onValidated,
    shouldReset,
    changeResetState,
    setReCaptchaToken,
    reCaptchaToken,
}: IChangePasswordFormProps) => {
    const [fields, handleChange] = useStateForModel({ ...defaultState });

    const { changePasswordForm, errorsPasswordForm, usePasswordGeneration, useEmail, showPasswordMeter, reCaptcha } =
        React.useContext(GlobalContext);

    const {
        currentPasswordHelpBlock,
        currentPasswordLabel,
        newPasswordHelpBlock,
        newPasswordLabel,
        newPasswordVerifyHelpBlock,
        newPasswordVerifyLabel,
        usernameDefaultDomainHelperBlock,
        usernameHelpBlock,
        usernameLabel,
    } = changePasswordForm;

    const { fieldRequired, passwordMatch, usernameEmailPattern, usernamePattern } = errorsPasswordForm;

    const userNameValidations = ['required', useEmail ? 'isUserEmail' : 'isUserName'];
    //const userNameValidations = ['required', useEmail];
    const userNameErrorMessages = [fieldRequired, useEmail ? usernameEmailPattern : usernamePattern];
    const userNameHelperText = useEmail ? usernameHelpBlock : usernameDefaultDomainHelperBlock;

    //if (submitData) {
    //    toSubmitData(fields);
    //}
    React.useEffect(() => {
        if (submitData) {
            toSubmitData(fields);
        }
    }, [submitData]);

    React.useEffect(() => {
        if (parentRef.current !== null && parentRef.current.isFormValid !== null) {
            parentRef.current.isFormValid().then((response: any) => {
                let validated = response;
                if (reCaptcha.siteKey && reCaptcha.siteKey !== '') {
                    validated = validated && reCaptchaToken !== '';
                }
                onValidated(!validated);
            });
        }
    });

    React.useEffect(() => {
        if (shouldReset) {
            handleChange({ ...defaultState });
            changeResetState(false);
            if (parentRef.current && parentRef.current.resetValidations) {
                parentRef.current.resetValidations();
            }
        }
    }, [shouldReset]);

    const setGenerated = (password: any) =>
        handleChange({
            NewPassword: password,
            NewPasswordVerify: password,
        });

    return (
        <FormGroup row={false} style={{ width: '80%', margin: '15px 0 0 10%' }}>
            <TextValidator
                autoFocus={true}
                inputProps={{
                    tabIndex: 1,
                }}
                id="Username"
                label={usernameLabel}
                helperText={userNameHelperText}
                name="Username"
                onChange={handleChange}
                validators={userNameValidations}
                value={fields.Username}
                style={{
                    height: '20px',
                    margin: '15px 0 50px 0',
                }}
                fullWidth={true}
                errorMessages={userNameErrorMessages}
            />
            <TextValidator
                inputProps={{
                    tabIndex: 2,
                }}
                label={currentPasswordLabel}
                helperText={currentPasswordHelpBlock}
                id="CurrentPassword"
                name="CurrentPassword"
                onChange={handleChange}
                type="password"
                validators={['required']}
                value={fields.CurrentPassword}
                style={{
                    height: '20px',
                    marginBottom: '50px',
                }}
                fullWidth={true}
                errorMessages={[fieldRequired]}
            />
            {usePasswordGeneration ? (
                <PasswordGenerator value={fields.NewPassword} setValue={setGenerated} />
            ) : (
                <>
                    <TextValidator
                        inputProps={{
                            tabIndex: 3,
                        }}
                        label={newPasswordLabel}
                        id="NewPassword"
                        name="NewPassword"
                        onChange={handleChange}
                        type="password"
                        validators={['required']}
                        value={fields.NewPassword}
                        style={{
                            height: '20px',
                            marginBottom: '30px',
                        }}
                        fullWidth={true}
                        errorMessages={[fieldRequired]}
                    />
                    {showPasswordMeter && <PasswordStrengthBar newPassword={fields.NewPassword} />}
                    <div
                        dangerouslySetInnerHTML={{ __html: newPasswordHelpBlock }}
                        style={{ font: '12px Roboto,Helvetica, Arial, sans-serif', marginBottom: '15px' }}
                    />
                    <TextValidator
                        inputProps={{
                            tabIndex: 4,
                        }}
                        label={newPasswordVerifyLabel}
                        helperText={newPasswordVerifyHelpBlock}
                        id="NewPasswordVerify"
                        name="NewPasswordVerify"
                        onChange={handleChange}
                        type="password"
                        validators={['required', `isPasswordMatch:${fields.NewPassword}`]}
                        value={fields.NewPasswordVerify}
                        style={{
                            height: '20px',
                            marginBottom: '50px',
                        }}
                        fullWidth={true}
                        errorMessages={[fieldRequired, passwordMatch]}
                    />
                </>
            )}

            {reCaptcha.siteKey && reCaptcha.siteKey !== '' && (
                <ReCaptcha setToken={setReCaptchaToken} shouldReset={false} />
            )}
        </FormGroup>
    );
};
