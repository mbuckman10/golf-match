import { useState } from 'react';
import { Button, Card, CardHeader, Body1Strong } from '@fluentui/react-components';
import { styles as useStyles } from './RoundRobinSetupPage.styles';

interface RoundRobinSetupPageProps {
  matchId: number;
  betConfigId: number;
  onCalculate: (config: RoundRobinConfig) => Promise<void>;
  isLoading?: boolean;
}

interface RoundRobinConfig {
  betConfigId: number;
}

export const RoundRobinSetupPage: React.FC<RoundRobinSetupPageProps> = ({
  matchId: _matchId,
  betConfigId,
  onCalculate,
  isLoading = false
}) => {
  const styles = useStyles();
  const [config] = useState<RoundRobinConfig>({
    betConfigId
  });

  const handleCalculate = async () => {
    await onCalculate(config);
  };

  return (
    <div className={styles.container}>
      <Card>
        <CardHeader header={<Body1Strong>Round Robin Setup</Body1Strong>} />
        <div>
          <div className={styles.content}>
            <p>
              A Round Robin matches every team (or player) against every other team in your group.
            </p>
            <p>
              Click Calculate to generate all pairwise matchups and compute results.
            </p>

            <div className={styles.actions}>
              <Button
                appearance="primary"
                onClick={handleCalculate}
                disabled={isLoading}
              >
                {isLoading ? 'Calculating...' : 'Calculate Round Robin'}
              </Button>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};

export default RoundRobinSetupPage;
