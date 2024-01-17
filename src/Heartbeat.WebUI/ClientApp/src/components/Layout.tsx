import React, { Component, forwardRef, useState } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import Snackbar from '@mui/material/Snackbar';
import MuiAlert, { AlertProps } from '@mui/material/Alert';

type Props = {
  children?: string | JSX.Element
}

export const Layout = ({children} : Props) => {
  const [showErrorMessage, setShowErrorMessage] = useState(true)
  const [errorMessage, setErrorMessage] = useState('This is an error message!')

  const handleCloseErrorMessage = (event?: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') {
      return;
    }

    setShowErrorMessage(false);
  };

  return (
    <div>
      <NavMenu />
      <Container tag="main">
        {children}
      </Container>
      <Snackbar open={showErrorMessage} autoHideDuration={6000} anchorOrigin={{ vertical: 'top', horizontal: 'right' }} onClose={handleCloseErrorMessage}>
        <MuiAlert elevation={6} variant="filled" severity="error" sx={{ width: '100%' }} onClose={handleCloseErrorMessage}>
          {errorMessage}
        </MuiAlert>
      </Snackbar>
    </div>
  );
}
