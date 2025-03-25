import {useEffect, useState} from "react";
import {validate} from "../../api-handlers/authHandler";
import {useNavigate} from "react-router-dom";

const AuthLayout = ({ children }) => {
    const navigate = useNavigate();
    const [isAuthenticated, setIsAuthenticated] = useState(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const validateUser = async () => {
            const result = await validate();
            setIsAuthenticated(result);
            setIsLoading(false);
        }

        validateUser();
    }, []);

    useEffect(() => {
        if (!isLoading && !isAuthenticated) {
            navigate("/login");
        }
    }, [isLoading, isAuthenticated, navigate]);

    if (isLoading) {
        return <div>Загрузка...</div>
    }

    if (!isAuthenticated) {
        navigate("/login")
    }

    return (
        <>
            {children}
        </>
    )
}

export default AuthLayout;