using System.Collections.Generic;
using FluentAssertions;
using Momento.Sdk.Auth.AccessControl;
using Xunit;

[assembly: TestFramework("Momento.Sdk.Tests.Unit.MomentoXunitFramework", "Momento.Sdk.Tests.Unit")]

namespace Momento.Sdk.Tests.Unit.Auth.AccessControl;

public class DisposableTokenScopeTest
{
    [Fact]
    public void DisposableTokenScopes_CacheReadWrite()
    {
        // CacheSelector.ByName("foo").Should().BeEquivalentTo(CacheSelector.ByName("foo"));
        // // Assert.Equal(CacheSelector.ByName("foo"), CacheSelector.ByName("bar"));
        // CacheItemSelector.AllCacheItems.Should().BeEquivalentTo(CacheItemSelector.AllCacheItems);
        // //
        // // new DisposableTokenPermission.CachePermission(CacheRole.ReadOnly, CacheSelector.ByName("taco"))
        // //     .Should().BeEquivalentTo(
        // //         new DisposableTokenPermission.CachePermission(CacheRole.ReadOnly, CacheSelector.ByName("burrito")),
        // //         o => o.ComparingRecordsByValue());
        // new DisposableTokenPermission.CachePermission(CacheRole.ReadOnly, CacheSelector.ByName("taco"))
        //     .Should().BeEquivalentTo(
        //         new DisposableTokenPermission.CachePermission(CacheRole.ReadOnly, CacheSelector.ByName("burrito")));
        
        //
        // Assert.Equal(
        //     new DisposableTokenPermission.CachePermission(CacheRole.ReadOnly, CacheSelector.ByName("taco")),
        //     new DisposableTokenPermission.CachePermission(CacheRole.ReadOnly, CacheSelector.ByName("burrito"))
        // );

        var scope = DisposableTokenScopes.CacheReadWrite("mycache");
        // Assert.Equal(scope, new DisposableTokenScope(new List<DisposableTokenPermission>
        // {
        //     new DisposableTokenPermission.CacheItemPermission(
        //         CacheRole.ReadWrite,
        //         CacheSelector.ByName("mycache2"),
        //         CacheItemSelector.AllCacheItems
        //     )
        // }));
        scope.Should().BeEquivalentTo(new DisposableTokenScope(new List<DisposableTokenPermission>
        {
            new DisposableToken.CacheItemPermission(
                CacheRole.ReadWrite,
                CacheSelector.ByName("mycache"),
                CacheItemSelector.AllCacheItems
            )
        })
            // , 
            // // o => o.ComparingByMembers<DisposableTokenScope>()
            // o => o
            );
    }
    
    //
    //   describe('should support assignment from DisposableTokenScope factory functions', () => {
  //   it('cacheReadWrite', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.cacheReadWrite('mycache');
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.ReadWrite, cache: 'mycache'}],
  //     });
  //     scope = DisposableTokenScopes.cacheReadWrite(AllCaches);
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.ReadWrite, cache: AllCaches}],
  //     });
  //     scope = DisposableTokenScopes.cacheReadWrite({name: 'mycache'});
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.ReadWrite, cache: {name: 'mycache'}}],
  //     });
  //   });
  //   it('cacheReadOnly', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.cacheReadOnly('mycache');
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.ReadOnly, cache: 'mycache'}],
  //     });
  //     scope = DisposableTokenScopes.cacheReadOnly(AllCaches);
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.ReadOnly, cache: AllCaches}],
  //     });
  //     scope = DisposableTokenScopes.cacheReadOnly({name: 'mycache'});
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.ReadOnly, cache: {name: 'mycache'}}],
  //     });
  //   });
  //   it('cacheWriteOnly', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.cacheWriteOnly('mycache');
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.WriteOnly, cache: 'mycache'}],
  //     });
  //     scope = DisposableTokenScopes.cacheWriteOnly(AllCaches);
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.WriteOnly, cache: AllCaches}],
  //     });
  //     scope = DisposableTokenScopes.cacheWriteOnly({name: 'mycache'});
  //     expect(scope).toEqual({
  //       permissions: [{role: CacheRole.WriteOnly, cache: {name: 'mycache'}}],
  //     });
  //   });
  //
  //   it('cacheKeyReadOnly', () => {
  //     let scope: DisposableTokenScope = DisposableTokenScopes.cacheKeyReadOnly(
  //       'mycache',
  //       'mykey'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.ReadOnly, cache: 'mycache', item: 'mykey'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyReadOnly(AllCaches, 'mykey');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.ReadOnly, cache: AllCaches, item: 'mykey'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyReadOnly(
  //       {name: 'mycache'},
  //       'mykey'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.ReadOnly, cache: {name: 'mycache'}, item: 'mykey'},
  //       ],
  //     });
  //   });
  //
  //   it('cacheKeyReadWrite', () => {
  //     let scope: DisposableTokenScope = DisposableTokenScopes.cacheKeyReadWrite(
  //       'mycache',
  //       'mykey'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.ReadWrite, cache: 'mycache', item: 'mykey'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyReadWrite(AllCaches, 'mykey');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.ReadWrite, cache: AllCaches, item: 'mykey'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyReadWrite(
  //       {name: 'mycache'},
  //       'mykey'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.ReadWrite, cache: {name: 'mycache'}, item: 'mykey'},
  //       ],
  //     });
  //   });
  //
  //   it('cacheKeyWriteOnly', () => {
  //     let scope: DisposableTokenScope = DisposableTokenScopes.cacheKeyWriteOnly(
  //       'mycache',
  //       'mykey'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.WriteOnly, cache: 'mycache', item: 'mykey'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyWriteOnly(AllCaches, 'mykey');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: CacheRole.WriteOnly, cache: AllCaches, item: 'mykey'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyWriteOnly(
  //       {name: 'mycache'},
  //       'mykey'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.WriteOnly,
  //           cache: {name: 'mycache'},
  //           item: 'mykey',
  //         },
  //       ],
  //     });
  //   });
  //
  //   it('cacheKeyPrefixReadOnly', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.cacheKeyPrefixReadOnly('mycache', 'mykeyprefix');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.ReadOnly,
  //           cache: 'mycache',
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyPrefixReadOnly(
  //       AllCaches,
  //       'mykeyprefix'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.ReadOnly,
  //           cache: AllCaches,
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyPrefixReadOnly(
  //       {name: 'mycache'},
  //       'mykeyprefix'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.ReadOnly,
  //           cache: {name: 'mycache'},
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //   });
  //
  //   it('cacheKeyPrefixReadWrite', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.cacheKeyPrefixReadWrite('mycache', 'mykeyprefix');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.ReadWrite,
  //           cache: 'mycache',
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyPrefixReadWrite(
  //       AllCaches,
  //       'mykeyprefix'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.ReadWrite,
  //           cache: AllCaches,
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyPrefixReadWrite(
  //       {name: 'mycache'},
  //       'mykeyprefix'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.ReadWrite,
  //           cache: {name: 'mycache'},
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //   });
  //
  //   it('cacheKeyPrefixWriteOnly', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.cacheKeyPrefixWriteOnly('mycache', 'mykeyprefix');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.WriteOnly,
  //           cache: 'mycache',
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyPrefixWriteOnly(
  //       AllCaches,
  //       'mykeyprefix'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.WriteOnly,
  //           cache: AllCaches,
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.cacheKeyPrefixWriteOnly(
  //       {name: 'mycache'},
  //       'mykeyprefix'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: CacheRole.WriteOnly,
  //           cache: {name: 'mycache'},
  //           item: {keyPrefix: 'mykeyprefix'},
  //         },
  //       ],
  //     });
  //   });
  //
  //   it('topicSubscribeOnly', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.topicSubscribeOnly('mycache', 'mytopic');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: TopicRole.SubscribeOnly, cache: 'mycache', topic: 'mytopic'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.topicSubscribeOnly(AllCaches, AllTopics);
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: TopicRole.SubscribeOnly, cache: AllCaches, topic: AllTopics},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.topicSubscribeOnly(
  //       {name: 'mycache'},
  //       {name: 'mytopic'}
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: TopicRole.SubscribeOnly,
  //           cache: {name: 'mycache'},
  //           topic: {name: 'mytopic'},
  //         },
  //       ],
  //     });
  //   });
  //   it('topicPublishOnly', () => {
  //     let scope: DisposableTokenScope = DisposableTokenScopes.topicPublishOnly(
  //       'mycache',
  //       'mytopic'
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: TopicRole.PublishOnly, cache: 'mycache', topic: 'mytopic'},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.topicPublishOnly(AllCaches, AllTopics);
  //     expect(scope).toEqual({
  //       permissions: [
  //         {role: TopicRole.PublishOnly, cache: AllCaches, topic: AllTopics},
  //       ],
  //     });
  //     scope = DisposableTokenScopes.topicPublishOnly(
  //       {name: 'mycache'},
  //       {name: 'mytopic'}
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: TopicRole.PublishOnly,
  //           cache: {name: 'mycache'},
  //           topic: {name: 'mytopic'},
  //         },
  //       ],
  //     });
  //   });
  //   it('topicPublishSubscribe', () => {
  //     let scope: DisposableTokenScope =
  //       DisposableTokenScopes.topicPublishSubscribe('mycache', 'mytopic');
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: TopicRole.PublishSubscribe,
  //           cache: 'mycache',
  //           topic: 'mytopic',
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.topicPublishSubscribe(AllCaches, AllTopics);
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: TopicRole.PublishSubscribe,
  //           cache: AllCaches,
  //           topic: AllTopics,
  //         },
  //       ],
  //     });
  //     scope = DisposableTokenScopes.topicPublishSubscribe(
  //       {name: 'mycache'},
  //       {name: 'mytopic'}
  //     );
  //     expect(scope).toEqual({
  //       permissions: [
  //         {
  //           role: TopicRole.PublishSubscribe,
  //           cache: {name: 'mycache'},
  //           topic: {name: 'mytopic'},
  //         },
  //       ],
  //     });
  //   });
  // })
    //
}