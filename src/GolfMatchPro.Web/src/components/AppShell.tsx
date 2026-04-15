import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Hamburger,
  DrawerBody,
  DrawerHeader,
  DrawerHeaderTitle,
  InlineDrawer,
  OverlayDrawer,
  NavDrawerBody,
  NavDrawerHeader,
  NavItem,
  NavSectionHeader,
} from '@fluentui/react-components';
import {
  bundleIcon,
  Home24Regular,
  Home24Filled,
  SportBaseball24Regular,
  SportBaseball24Filled,
  People24Regular,
  People24Filled,
  Trophy24Regular,
  Trophy24Filled,
  Settings24Regular,
  Settings24Filled,
  Navigation24Regular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
  root: {
    display: 'flex',
    height: '100vh',
    overflow: 'hidden',
  },
  nav: {
    width: '240px',
    flexShrink: 0,
    borderRight: `1px solid ${tokens.colorNeutralStroke1}`,
    '@media (max-width: 768px)': {
      display: 'none',
    },
  },
  mobileHeader: {
    display: 'none',
    padding: '8px 16px',
    borderBottom: `1px solid ${tokens.colorNeutralStroke1}`,
    alignItems: 'center',
    gap: '8px',
    '@media (max-width: 768px)': {
      display: 'flex',
    },
  },
  content: {
    flex: 1,
    overflow: 'auto',
    display: 'flex',
    flexDirection: 'column',
  },
  main: {
    flex: 1,
    padding: '24px',
    '@media (max-width: 768px)': {
      padding: '16px',
    },
  },
  navHeader: {
    padding: '16px',
    fontWeight: tokens.fontWeightSemibold,
    fontSize: tokens.fontSizeBase500,
    color: tokens.colorBrandForeground1,
  },
  navList: {
    listStyle: 'none',
    padding: 0,
    margin: 0,
  },
  navItem: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
    padding: '10px 16px',
    cursor: 'pointer',
    color: tokens.colorNeutralForeground1,
    textDecoration: 'none',
    borderRadius: tokens.borderRadiusMedium,
    margin: '2px 8px',
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  navItemActive: {
    backgroundColor: tokens.colorBrandBackground2,
    color: tokens.colorBrandForeground1,
    fontWeight: tokens.fontWeightSemibold,
  },
  title: {
    fontSize: tokens.fontSizeBase500,
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorBrandForeground1,
  },
});

interface NavLinkItem {
  path: string;
  label: string;
  icon: string;
}

const navItems: NavLinkItem[] = [
  { path: '/', label: 'Dashboard', icon: 'home' },
  { path: '/courses', label: 'Courses', icon: 'course' },
  { path: '/players', label: 'Players', icon: 'players' },
  { path: '/matches', label: 'Matches', icon: 'match' },
];

function getIcon(icon: string) {
  switch (icon) {
    case 'home': return <Home24Regular />;
    case 'course': return <SportBaseball24Regular />;
    case 'players': return <People24Regular />;
    case 'match': return <Trophy24Regular />;
    default: return <Home24Regular />;
  }
}

interface AppShellProps {
  children: React.ReactNode;
}

export function AppShell({ children }: AppShellProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const styles = useStyles();
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  const isActive = (path: string) => {
    if (path === '/') return location.pathname === '/';
    return location.pathname.startsWith(path);
  };

  const handleNav = (path: string) => {
    navigate(path);
    setMobileNavOpen(false);
  };

  const renderNavItems = () => (
    <ul className={styles.navList}>
      {navItems.map(item => (
        <li key={item.path}>
          <div
            className={`${styles.navItem} ${isActive(item.path) ? styles.navItemActive : ''}`}
            onClick={() => handleNav(item.path)}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => e.key === 'Enter' && handleNav(item.path)}
          >
            {getIcon(item.icon)}
            <span>{item.label}</span>
          </div>
        </li>
      ))}
    </ul>
  );

  return (
    <div className={styles.root}>
      {/* Desktop sidebar */}
      <nav className={styles.nav}>
        <div className={styles.navHeader}>Golf Match Pro</div>
        {renderNavItems()}
      </nav>

      {/* Mobile overlay drawer */}
      {mobileNavOpen && (
        <div
          style={{
            position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.4)', zIndex: 1000
          }}
          onClick={() => setMobileNavOpen(false)}
        >
          <div
            style={{
              width: 260, height: '100%', backgroundColor: tokens.colorNeutralBackground1,
              boxShadow: tokens.shadow16
            }}
            onClick={e => e.stopPropagation()}
          >
            <div className={styles.navHeader}>Golf Match Pro</div>
            {renderNavItems()}
          </div>
        </div>
      )}

      <div className={styles.content}>
        {/* Mobile header */}
        <div className={styles.mobileHeader}>
          <Navigation24Regular
            style={{ cursor: 'pointer' }}
            onClick={() => setMobileNavOpen(true)}
          />
          <span className={styles.title}>Golf Match Pro</span>
        </div>

        <main className={styles.main}>
          {children}
        </main>
      </div>
    </div>
  );
}
