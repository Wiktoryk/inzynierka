:restart
mlagents-learn results\playeragent\configuration.yaml --run-id=playeragent --resume
if %ERRORLEVEL% neq 0 (
    echo "Training crashed. Restarting..."
    goto restart
)
