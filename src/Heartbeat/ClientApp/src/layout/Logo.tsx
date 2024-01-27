import MonitorHeart from '@mui/icons-material/MonitorHeart'
import {Stack, Typography} from "@mui/material";
import Box from "@mui/material/Box";

const Logo = () => {
    return (
        <Box display="flex" alignItems="center">
            <MonitorHeart />
            <Typography component="span" variant="h5">
                Heartbeat
            </Typography>
        </Box>
    );
};

export default Logo;
