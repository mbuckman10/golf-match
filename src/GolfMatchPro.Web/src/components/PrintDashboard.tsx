import { useState } from 'react';
import {
  Card,
  CardHeader,
  Button,
  Tab,
  TabList,
  Body1Strong
} from '@fluentui/react-components';
import { DocumentPdf20Regular, ArrowDownload20Regular } from '@fluentui/react-icons';
import { styles as useStyles } from './PrintDashboard.styles';

interface PrintDashboardProps {
  matchId: number;
  isLoading?: boolean;
}

type PrintViewType = 'scorecard' | 'results' | 'grand-totals' | 'full-report';

export const PrintDashboard: React.FC<PrintDashboardProps> = ({
  matchId,
  isLoading = false
}) => {
  const styles = useStyles();
  const [selectedView, setSelectedView] = useState<PrintViewType>('scorecard');

  const handleExportPDF = async (viewType: PrintViewType) => {
    try {
      const endpoint = `api/matches/${matchId}/export/${viewType === 'full-report' ? 'full-report' : viewType}`;
      const response = await fetch(endpoint);

      if (!response.ok) throw new Error('PDF export failed');

      // Create a blob from the response
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `golf-match-${matchId}-${viewType}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      console.error('Error exporting PDF:', error);
      alert('Failed to export PDF');
    }
  };

  const handlePrint = () => {
    window.print();
  };

  return (
    <div className={styles.container}>
      <Card>
        <CardHeader header={<Body1Strong>Print & Export</Body1Strong>} />
        <div>
          <div className={styles.content}>
            <p>Select a view to print or export as PDF:</p>

            <TabList selectedValue={selectedView} onTabSelect={(_, data) => setSelectedView(data.value as PrintViewType)}>
              <Tab value="scorecard">Scorecard</Tab>
              <Tab value="results">Bet Results</Tab>
              <Tab value="grand-totals">Grand Totals</Tab>
              <Tab value="full-report">Full Report</Tab>
            </TabList>

            <div className={styles.actions}>
              <Button
                icon={<ArrowDownload20Regular />}
                appearance="primary"
                onClick={() => handleExportPDF(selectedView)}
                disabled={isLoading}
              >
                Export as PDF
              </Button>
              <Button
                icon={<DocumentPdf20Regular />}
                onClick={handlePrint}
                disabled={isLoading}
              >
                Print
              </Button>
            </div>

            <div className={styles.preview}>
              <PrintPreview viewType={selectedView} matchId={matchId} />
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};

interface PrintPreviewProps {
  viewType: PrintViewType;
  matchId: number;
}

const PrintPreview: React.FC<PrintPreviewProps> = ({ viewType, matchId }) => {
  const styles = useStyles();
  return (
    <div className={styles.printPreviewContainer}>
      <h3>Preview: {viewType.replace('-', ' ').toUpperCase()}</h3>
      <div className={styles.printPreview}>
        {viewType === 'scorecard' && <ScorecardPreview matchId={matchId} />}
        {viewType === 'results' && <ResultsPreview matchId={matchId} />}
        {viewType === 'grand-totals' && <GrandTotalsPreview matchId={matchId} />}
        {viewType === 'full-report' && <FullReportPreview matchId={matchId} />}
      </div>
    </div>
  );
};

const ScorecardPreview: React.FC<{ matchId: number }> = ({ matchId }) => {
  const styles = useStyles();
  return (
    <div>
      <h4>Players & Scores</h4>
      <p>Course information and hole-by-hole scores for all players</p>
      <p className={styles.placeholder}>Scorecard preview for match {matchId}</p>
    </div>
  );
};

const ResultsPreview: React.FC<{ matchId: number }> = ({ matchId }) => {
  const styles = useStyles();
  return (
    <div>
      <h4>Bet Results</h4>
      <p>Foursomes, Threesomes, Individual, and Best Ball results</p>
      <p className={styles.placeholder}>Results preview for match {matchId}</p>
    </div>
  );
};

const GrandTotalsPreview: React.FC<{ matchId: number }> = ({ matchId }) => {
  const styles = useStyles();
  return (
    <div>
      <h4>Grand Totals</h4>
      <p>Aggregate winnings/losses per player across all bets</p>
      <p className={styles.placeholder}>Grand totals preview for match {matchId}</p>
    </div>
  );
};

const FullReportPreview: React.FC<{ matchId: number }> = ({ matchId }) => {
  const styles = useStyles();
  return (
    <div>
      <h4>Full Report</h4>
      <p>Complete scorecard, all bet results, and grand totals</p>
      <p className={styles.placeholder}>Full report preview for match {matchId}</p>
    </div>
  );
};

export default PrintDashboard;
