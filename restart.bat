:restart
mlagents-learn results\player\configuration.yaml --run-id=player --resume
if %ERRORLEVEL% neq 0 (
    echo "Training crashed. Restarting..."
    goto restart
)
