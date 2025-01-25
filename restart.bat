:restart
mlagents-learn results\companionv3\configuration.yaml --run-id=companionv3 --resume --no-graphics
if %ERRORLEVEL% neq 0 (
    echo "Training crashed. Restarting..."
    goto restart
)
