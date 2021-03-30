import * as React from 'react';
import Grid from '@material-ui/core/Grid/Grid';
import Typography from '@material-ui/core/Typography/Typography';
//import mitLogo from '../assets/images/License_icon-mit.svg.png';
//import uslogo from '../assets/images/logo.png';
//import osiLogo from '../assets/images/osi.png';
//import passcoreLogo from '../assets/images/passcore-logo.png';

export const Footer: React.FunctionComponent<any> = () => (
    <div
        style={{
            marginTop: '40px',
            width: '650px',
        }}
    >
        <Grid alignItems="center" container={true} direction="row" justify="space-between">
            
        </Grid>
        <Grid alignItems="center" container={true} direction="column" justify="space-evenly">
            <Typography align="center" variant="h6">
                Công ty Cổ phần Đầu tư và Kinh doanh Bất động sản Hải Phát
            </Typography>
            <Typography align="center" variant="caption">
                Liên hệ hộ trợ: Email: <a href="mailto:bancongnghe@haiphatland.com.vn">bancongnghe@haiphatland.com.vn</a>. Hotline: <a href="tel:0985596901">0985596901</a>
            </Typography>
        </Grid>
    </div>
);
