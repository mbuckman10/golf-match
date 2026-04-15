import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import { AppShell } from './components/AppShell';
import { DashboardPage } from './pages/DashboardPage';
import { CoursesPage } from './pages/CoursesPage';
import { CourseEditorPage } from './pages/CourseEditorPage';
import { PlayersPage } from './pages/PlayersPage';
import { PlayerEditorPage } from './pages/PlayerEditorPage';
import { MatchDashboardPage } from './pages/MatchDashboardPage';
import { CreateMatchPage } from './pages/CreateMatchPage';
import { MatchDetailPage } from './pages/MatchDetailPage';
import { ScorecardPage } from './pages/ScorecardPage';
import { BetConfigPage } from './pages/BetConfigPage';
import { BetResultsPage } from './pages/BetResultsPage';
import { IndividualResultsPage } from './pages/IndividualResultsPage';
import { BestBallResultsPage } from './pages/BestBallResultsPage';
import { BestBallSummaryPage } from './pages/BestBallSummaryPage';

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <BrowserRouter>
        <AppShell>
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/courses" element={<CoursesPage />} />
            <Route path="/courses/new" element={<CourseEditorPage />} />
            <Route path="/courses/:id" element={<CourseEditorPage />} />
            <Route path="/players" element={<PlayersPage />} />
            <Route path="/players/new" element={<PlayerEditorPage />} />
            <Route path="/players/:id" element={<PlayerEditorPage />} />
            <Route path="/matches" element={<MatchDashboardPage />} />
            <Route path="/matches/new" element={<CreateMatchPage />} />
            <Route path="/matches/:id" element={<MatchDetailPage />} />
            <Route path="/matches/:id/scorecard" element={<ScorecardPage />} />
            <Route path="/matches/:id/bets" element={<BetConfigPage />} />
            <Route path="/matches/:id/bets/:betConfigId/results" element={<BetResultsPage />} />
            <Route path="/matches/:id/bets/:betConfigId/individual-results" element={<IndividualResultsPage />} />
            <Route path="/matches/:id/bets/:betConfigId/bestball-results" element={<BestBallResultsPage />} />
            <Route path="/matches/:id/bestball-summary" element={<BestBallSummaryPage />} />
          </Routes>
        </AppShell>
      </BrowserRouter>
    </FluentProvider>
  );
}

export default App;
