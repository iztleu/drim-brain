'use client';

import '@mantine/core/styles.css';

import { AppShell, Burger, ColorSchemeScript, Container, Group, MantineProvider, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import classes from './HeaderMenu.module.css';
import Link from 'next/link';

const links = [
  { link: '/products', label: 'Products' },
  { link: '/orders', label: 'Orders' },
  { link: '/cart', label: 'Cart' },
];

export default function ({
  children,
}: Readonly<{children: React.ReactNode}>) {
  const [opened, { toggle }] = useDisclosure();

  const items = links.map((link) => {
    return (
      <Link
        key={link.label}
        href={link.link}
        className={classes.link}
      >
        {link.label}
      </Link>
    );
  });

  return (
    <html lang="en">
      <head>
        <ColorSchemeScript />
      </head>
      <body>
        <MantineProvider>
          <AppShell
            header={{ height: 60 }}
            navbar={{
              width: 300,
              breakpoint: 'sm',
              collapsed: { mobile: !opened },
            }}
            padding="md"
          >
            <header className={classes.header}>
              <Container size="md">
                <div className={classes.inner}>
                  <Title>StripeLab</Title>
                  <Group gap={70} visibleFrom="sm">
                    {items}
                  </Group>
                  <Burger opened={opened} onClick={toggle} size="sm" hiddenFrom="sm" />
                </div>
              </Container>
            </header>
            <AppShell.Main>{children}</AppShell.Main> 
          </AppShell>
        </MantineProvider>
      </body>
    </html>
  );
}
