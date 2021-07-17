import * as React from 'react';
import { MessageType, SnackbarContainer, snackbarService } from 'uno-material-ui';
import { SnackbarContext } from './GlobalContext';

interface ISnackbarProviderProps {
    children: any;
}

export const SnackbarContextProvider: React.FunctionComponent<ISnackbarProviderProps> = ({
    children,
}: ISnackbarProviderProps) => {
    const [providerValue] = React.useState({
        sendMessage: (messageText: string, messageType = 'success') =>
            snackbarService.showSnackbar(messageText, messageType as MessageType),
    });

    return (
        <SnackbarContext.Provider value={providerValue}>
            {children}
            <SnackbarContainer />
        </SnackbarContext.Provider>
    );
};
