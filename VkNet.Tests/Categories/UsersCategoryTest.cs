﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using VkNet.Categories;
using VkNet.Enums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.Tests.Categories
{
    [TestFixture]
    public class UsersCategoryTest
    {
        private const string Query = "Masha Ivanova";

        [SetUp]
        public void SetUp()
        {
        
        }

        private UsersCategory GetMockedUsersCategory(string url, string json)
        {
            var browser = new Mock<IBrowser>();
            browser.Setup(m => m.GetJson(url)).Returns(json);

            return new UsersCategory(new VkApi { AccessToken = "token", Browser = browser.Object, Version = "5.9"});
        }

        [Test]
        public void Get_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var users = new UsersCategory(new VkApi());
            ExceptionAssert.Throws<AccessTokenInvalidException>(() => users.Get(1));
        }

        [Test]
        public void Get_NotAccessToInternet_ThrowVkApiException()
        {   
            var mockBrowser = new Mock<IBrowser>();
            mockBrowser.Setup(f => f.GetJson(It.IsAny<string>())).Throws(new VkApiException("The remote name could not be resolved: 'api.vk.com'"));

            var users = new UsersCategory(new VkApi {AccessToken = "asgsstsfast", Browser = mockBrowser.Object});

            var ex = ExceptionAssert.Throws<VkApiException>(() => users.Get(1));
            ex.Message.ShouldEqual("The remote name could not be resolved: 'api.vk.com'");
        }

        [Test]
        public void Get_WrongAccesToken_Throw_ThrowUserAuthorizationException()
        {
            const string url = "https://api.vk.com/method/users.get?v=5.9&user_ids=1&access_token=token";

            const string json =
                @"{
                    'error': {
                      'error_code': 5,
                      'error_msg': 'User authorization failed: invalid access_token.',
                      'request_params': [
                        {
                          'key': 'oauth',
                          'value': '1'
                        },
                        {
                          'key': 'method',
                          'value': 'getProfiles'
                        },
                        {
                          'key': 'uid',
                          'value': '1'
                        },
                        {
                          'key': 'access_token',
                          'value': 'sfastybdsjhdg'
                        }
                      ]
                    }
                  }";

            var users = GetMockedUsersCategory(url, json);
            var ex = ExceptionAssert.Throws<UserAuthorizationFailException>(() => users.Get(1));
            ex.Message.ShouldEqual("User authorization failed: invalid access_token.");
        }

        [Test]
        public void Get_WithSomeFields_FirstNameLastNameEducation()
        {
            const string url = "https://api.vk.com/method/users.get?fields=first_name,last_name,education&v=5.9&user_ids=1&access_token=token";
            const string json =
                @"{
                    'response': [
                      {
                        'id': 1,
                        'first_name': 'Павел',
                        'last_name': 'Дуров',
                        'university': 1,
                        'university_name': 'СПбГУ',
                        'faculty': 0,
                        'faculty_name': '',
                        'graduation': 2006
                      }
                    ]
                  }";

            UsersCategory users = GetMockedUsersCategory(url, json);

            // act
            var fields = ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Education;
            User p = users.Get(1, fields);

            // assert
            Assert.That(p, Is.Not.Null);
            Assert.That(p.Id, Is.EqualTo(1));
            Assert.That(p.FirstName, Is.EqualTo("Павел"));
            Assert.That(p.LastName, Is.EqualTo("Дуров"));
            Assert.That(p.Education, Is.Not.Null);
            Assert.That(p.Education.UniversityId, Is.EqualTo(1));
            Assert.That(p.Education.UniversityName, Is.EqualTo("СПбГУ"));
            Assert.That(p.Education.FacultyId, Is.Null);
            Assert.That(p.Education.FacultyName, Is.EqualTo(""));
            Assert.That(p.Education.Graduation, Is.EqualTo(2006));
        }

        [Test]
        [Ignore("Obsolete test. Old api version")]
        public void Get_CountersFields_CountersObject()
        {
            const string url = "https://api.vk.com/method/getProfiles?uid=4793858&fields=counters&access_token=token";

            const string json =
                @"{
                    'response': [
                      {
                        'uid': 4793858,
                        'first_name': 'Антон',
                        'last_name': 'Жидков',
                        'counters': {
                          'albums': 1,
                          'videos': 100,
                          'audios': 153,
                          'notes': 3,
                          'photos': 54,
                          'groups': 40,
                          'friends': 371,
                          'online_friends': 44,
                          'mutual_friends': 2,
                          'user_photos': 164,
                          'user_videos': 87,
                          'followers': 1,
                          'subscriptions': 1,
                          'pages': 1
                        }
                      }
                    ]
                  }";

            var users = GetMockedUsersCategory(url, json);
            // act
            User p = users.Get(4793858, ProfileFields.Counters);

            // assert
            Assert.That(p, Is.Not.Null);
            Assert.That(p.Id, Is.EqualTo(4793858));
            Assert.That(p.FirstName, Is.EqualTo("Антон"));
            Assert.That(p.LastName, Is.EqualTo("Жидков"));
            Assert.That(p.Counters, Is.Not.Null);
            Assert.That(p.Counters.Albums, Is.EqualTo(1));
            Assert.That(p.Counters.Videos, Is.EqualTo(100));
            Assert.That(p.Counters.Audios, Is.EqualTo(153));
            Assert.That(p.Counters.Notes, Is.EqualTo(3));
            Assert.That(p.Counters.Photos, Is.EqualTo(54));
            Assert.That(p.Counters.Groups, Is.EqualTo(40));
            Assert.That(p.Counters.Friends, Is.EqualTo(371));
            Assert.That(p.Counters.OnlineFriends, Is.EqualTo(44));
            Assert.That(p.Counters.MutualFriends, Is.EqualTo(2));
            Assert.That(p.Counters.UserPhotos, Is.EqualTo(164));
            Assert.That(p.Counters.UserVideos, Is.EqualTo(87));
            Assert.That(p.Counters.Followers, Is.EqualTo(1));
            Assert.That(p.Counters.Subscriptions, Is.EqualTo(1));
            Assert.That(p.Counters.Pages, Is.EqualTo(1));
        }

        [Test]
        [Ignore("Obsolete test. Old api version")]
        public void Get_DefaultFields_UidFirstNameLastName()
        {
            const string url = "https://api.vk.com/method/getProfiles?uid=4793858&access_token=token";
            const string json =
                @"{
                    'response': [
                      {
                        'uid': 4793858,
                        'first_name': 'Антон',
                        'last_name': 'Жидков'
                      }
                    ]
                  }";

            var users = GetMockedUsersCategory(url, json);

            // act
            User p = users.Get(4793858);

            // assert
            Assert.That(p.Id, Is.EqualTo(4793858));
            Assert.That(p.FirstName, Is.EqualTo("Антон"));
            Assert.That(p.LastName, Is.EqualTo("Жидков"));
        }

        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void GetGropus_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var users = new UsersCategory(new VkApi());
            users.GetGroups(1);
        }

        [Test]
        [ExpectedException(typeof(AccessDeniedException), ExpectedMessage = "Access to the groups list is denied due to the user privacy settings.")]
        public void GetGroups_AccessDenied_ThrowAccessDeniedException()
        {
            const string url = "https://api.vk.com/method/getGroups?uid=1&access_token=token";

            const string json =
                @"{
                    'error': {
                      'error_code': 260,
                      'error_msg': 'Access to the groups list is denied due to the user privacy settings.',
                      'request_params': [
                        {
                          'key': 'oauth',
                          'value': '1'
                        },
                        {
                          'key': 'method',
                          'value': 'getGroups'
                        },
                        {
                          'key': 'uid',
                          'value': '1'
                        },
                        {
                          'key': 'access_token',
                          'value': '2f3e43eb608a87632f68d140d82f5a9efa22f772f7765eb2f49f67514987c5e'
                        }
                      ]
                    }
                  }";

            var users = GetMockedUsersCategory(url, json);
            users.GetGroups(1);
        }

        [Test]
        public void GetGroups_UserHaveNoGroups_EmptyList()
        {
            // undone: check it later
            const string url = "https://api.vk.com/method/getGroups?uid=4793858&access_token=token";

            const string json =
                @"{
                    'response': []
                  }";

            var users = GetMockedUsersCategory(url, json);
            var groups = users.GetGroups(4793858).ToList();

            Assert.That(groups.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetGroups_AccessGranted_ListOfGroups()
        {
            const string url = "https://api.vk.com/method/getGroups?uid=4793858&access_token=token";

            const string json =
                @"{
                    'response': [
                      1,
                      15,
                      134,
                      1673
                    ]
                  }";

            var users = GetMockedUsersCategory(url, json);
            var groups = users.GetGroups(4793858).ToList();

            Assert.That(groups.Count, Is.EqualTo(4));
            Assert.That(groups[0].Id, Is.EqualTo(1));
            Assert.That(groups[1].Id, Is.EqualTo(15));
            Assert.That(groups[2].Id, Is.EqualTo(134));
            Assert.That(groups[3].Id, Is.EqualTo(1673));
        }
        
        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void Get_Multiple_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var users = new UsersCategory(new VkApi());
            users.Get(new long[] {1, 2});
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Get_EmptyListOfUids_ThrowArgumentNullException()
        {
            var users = new UsersCategory(new VkApi { AccessToken = "token" });
            users.Get(null);
        }

        [Test]
        public void Get_Mutliple_TwoUidsDefaultFields_TwoProfiles()
        {
            const string url = "https://api.vk.com/method/users.get?v=5.9&user_ids=1,672&access_token=token";
            const string json =
                @"{
                    'response': [
                      {
                        'id': 1,
                        'first_name': 'Павел',
                        'last_name': 'Дуров'
                      },
                      {
                        'id': 672,
                        'first_name': 'Кристина',
                        'last_name': 'Смирнова'
                      }
                    ]
                  }";


            var users = GetMockedUsersCategory(url, json);
            ReadOnlyCollection<User> lst = users.Get(new long[] {1, 672});

            Assert.That(lst.Count, Is.EqualTo(2));
            Assert.That(lst[0], Is.Not.Null);
            Assert.That(lst[0].Id, Is.EqualTo(1));
            Assert.That(lst[0].FirstName, Is.EqualTo("Павел"));
            Assert.That(lst[0].LastName, Is.EqualTo("Дуров"));

            Assert.That(lst[1], Is.Not.Null);
            Assert.That(lst[1].Id, Is.EqualTo(672));
            Assert.That(lst[1].FirstName, Is.EqualTo("Кристина"));
            Assert.That(lst[1].LastName, Is.EqualTo("Смирнова"));
        }

        [Test, Ignore("Obsolete")]
        public void Get_TwoUidsEducationField_TwoProfiles()
        {
            const string url =
                "https://api.vk.com/method/getProfiles?uids=102674754,5041431&fields=education&access_token=token";

            const string json =
                @"{
                    'response': [
                      {
                        'uid': 102674754,
                        'first_name': 'Artyom',
                        'last_name': 'Plotnikov',
                        'university': '431',
                        'university_name': 'ВолгГТУ',
                        'faculty': '3162',
                        'faculty_name': 'Электроники и вычислительной техники',
                        'graduation': '2010'
                      },
                      {
                        'uid': 5041431,
                        'first_name': 'Tayfur',
                        'last_name': 'Kaseev',
                        'university': '431',
                        'university_name': 'ВолгГТУ',
                        'faculty': '3162',
                        'faculty_name': 'Электроники и вычислительной техники',
                        'graduation': '2012'
                      }
                    ]
                  }";

            var users = GetMockedUsersCategory(url, json);
            ReadOnlyCollection<User> lst = users.Get(new long[] {102674754, 5041431}, ProfileFields.Education);

            Assert.That(lst.Count == 2);
            Assert.That(lst[0], Is.Not.Null);
            Assert.That(lst[0].Id, Is.EqualTo(102674754));
            Assert.That(lst[0].FirstName, Is.EqualTo("Artyom"));
            Assert.That(lst[0].LastName, Is.EqualTo("Plotnikov"));
            Assert.That(lst[0].Education, Is.Not.Null);
            Assert.That(lst[0].Education.UniversityId, Is.EqualTo(431));
            Assert.That(lst[0].Education.UniversityName, Is.EqualTo("ВолгГТУ"));
            Assert.That(lst[0].Education.FacultyId, Is.EqualTo(3162));
            Assert.That(lst[0].Education.FacultyName, Is.EqualTo("Электроники и вычислительной техники"));
            Assert.That(lst[0].Education.Graduation, Is.EqualTo(2010));

            Assert.That(lst[1], Is.Not.Null);
            Assert.That(lst[1].Id, Is.EqualTo(5041431));
            Assert.That(lst[1].FirstName, Is.EqualTo("Tayfur"));
            Assert.That(lst[1].LastName, Is.EqualTo("Kaseev"));
            Assert.That(lst[1].Education, Is.Not.Null);
            Assert.That(lst[1].Education.UniversityId, Is.EqualTo(431));
            Assert.That(lst[1].Education.UniversityName, Is.EqualTo("ВолгГТУ"));
            Assert.That(lst[1].Education.FacultyId, Is.EqualTo(3162));
            Assert.That(lst[1].Education.FacultyName, Is.EqualTo("Электроники и вычислительной техники"));
            Assert.That(lst[1].Education.Graduation, Is.EqualTo(2012));
        }
        
        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void GetGroupsFull_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var vk = new VkApi();
            vk.Users.GetGroupsFull();
        }

        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void GetGroupsFull_Multiple_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var vk = new VkApi();
            vk.Users.GetGroupsFull(new long[]{1, 2});
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetGroupsFull_NullGids_ThrowArgumentNullException()
        {
            var vk = new VkApi { AccessToken = "token" };
            vk.Users.GetGroupsFull(null);
        }

        [Test]
        public void GetGroupsFull_Mulitple_TwoGroups()
        {
            const string url = "https://api.vk.com/method/getGroupsFull?gids=29689780,33489538&access_token=token";

            const string json =
                @"{
                    'response': [
                      {
                        'id': 29689780,
                        'name': 'Art and Life ©',
                        'screen_name': 'art.and.life',
                        'is_closed': 0,
                        'type': 'page',
                        'photo': 'http://cs11003.userapi.com/g29689780/e_1bea6489.jpg',
                        'photo_medium': 'http://cs11003.userapi.com/g29689780/d_f50bf769.jpg',
                        'photo_big': 'http://cs11003.userapi.com/g29689780/a_1889c16e.jpg'
                      },
                      {
                        'id': 33489538,
                        'name': 'Английский как стиль жизни. Где перевод?',
                        'screen_name': 'english_for_adults',
                        'is_closed': 0,
                        'type': 'event',
                        'photo': 'http://cs5538.userapi.com/g33489538/e_1d36792d.jpg',
                        'photo_medium': 'http://cs5538.userapi.com/g33489538/d_caafe13e.jpg',
                        'photo_big': 'http://cs5538.userapi.com/g33489538/a_6d6f2525.jpg'
                      }
                    ]
                  }";

            var users = GetMockedUsersCategory(url, json);
            var groups = users.GetGroupsFull(new long[] { 29689780, 33489538 }).ToList();

            Assert.That(groups.Count, Is.EqualTo(2));
            Assert.That(groups[0], Is.Not.Null);
            Assert.That(groups[0].Id, Is.EqualTo(29689780));
            Assert.That(groups[0].Name, Is.EqualTo("Art and Life ©"));
            Assert.That(groups[0].ScreenName, Is.EqualTo("art.and.life"));
            Assert.That(groups[0].IsClosed, Is.EqualTo(GroupPublicity.Public));
            Assert.That(groups[0].IsAdmin, Is.False);
            Assert.That(groups[0].Type, Is.EqualTo(GroupType.Page));

            Assert.That(groups[1], Is.Not.Null);
            Assert.That(groups[1].Id, Is.EqualTo(33489538));
            Assert.That(groups[1].Name, Is.EqualTo("Английский как стиль жизни. Где перевод?"));
            Assert.That(groups[1].ScreenName, Is.EqualTo("english_for_adults"));
            Assert.That(groups[1].IsClosed, Is.EqualTo(GroupPublicity.Public));
            Assert.That(groups[1].IsAdmin, Is.False);
            Assert.That(groups[1].Type, Is.EqualTo(GroupType.Event));
        }

        [Test]
        public void GetGroupsFull_GroupsOfCurrentUser()
        {
            const string url = "https://api.vk.com/method/getGroupsFull?access_token=token";

            const string json =
                @"{
                    'response': [
                      {
                        'id': 29689780,
                        'name': 'Art and Life ©',
                        'screen_name': 'art.and.life',
                        'is_closed': 0,
                        'is_admin': 1,
                        'type': 'page',
                        'photo': 'http://cs11003.userapi.com/g29689780/e_1bea6489.jpg',
                        'photo_medium': 'http://cs11003.userapi.com/g29689780/d_f50bf769.jpg',
                        'photo_big': 'http://cs11003.userapi.com/g29689780/a_1889c16e.jpg'
                      },
                      {
                        'id': 33489538,
                        'name': 'Английский как стиль жизни. Где перевод?',
                        'screen_name': 'english_for_adults',
                        'is_closed': 0,
                        'type': 'event',
                        'photo': 'http://cs5538.userapi.com/g33489538/e_1d36792d.jpg',
                        'photo_medium': 'http://cs5538.userapi.com/g33489538/d_caafe13e.jpg',
                        'photo_big': 'http://cs5538.userapi.com/g33489538/a_6d6f2525.jpg'
                      }
                    ]
                  }";

            var users = GetMockedUsersCategory(url, json);
            var groups = users.GetGroupsFull().ToList();

            Assert.That(groups.Count, Is.EqualTo(2));
            Assert.That(groups[0], Is.Not.Null);
            Assert.That(groups[0].Id, Is.EqualTo(29689780));
            Assert.That(groups[0].Name, Is.EqualTo("Art and Life ©"));
            Assert.That(groups[0].ScreenName, Is.EqualTo("art.and.life"));
            Assert.That(groups[0].IsClosed, Is.EqualTo(GroupPublicity.Public));
            Assert.That(groups[0].IsAdmin, Is.True);
            Assert.That(groups[0].Type, Is.EqualTo(GroupType.Page));

            Assert.That(groups[1], Is.Not.Null);
            Assert.That(groups[1].Id, Is.EqualTo(33489538));
            Assert.That(groups[1].Name, Is.EqualTo("Английский как стиль жизни. Где перевод?"));
            Assert.That(groups[1].ScreenName, Is.EqualTo("english_for_adults"));
            Assert.That(groups[1].IsClosed, Is.EqualTo(GroupPublicity.Public));
            Assert.That(groups[1].IsAdmin, Is.False);
            Assert.That(groups[1].Type, Is.EqualTo(GroupType.Event));
        }

        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void IsAppUser_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var users = new UsersCategory(new VkApi());
            users.IsAppUser(1);
        }

        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void GetUserSettings_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var vk = new VkApi();
            vk.Users.GetUserSettings(100);
        }

        [Test]
        public void GetUserSettings_AccessToFriends_Return2()
        {
            const string url = "https://api.vk.com/method/getUserSettings?uid=1&access_token=token";

            const string json =
                @"{
                    'response': 2
                  }";

            var users = GetMockedUsersCategory(url, json);
            int settings = users.GetUserSettings(1);

            Assert.That(settings, Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(AccessTokenInvalidException))]
        public void Search_EmptyAccessToken_ThrowAccessTokenInvalidException()
        {
            var vk = new VkApi();
            int count;
            vk.Users.Search(Query, out count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Query can not be null or empty.")]
        public void Search_EmptyQuery_ThrowArgumentException()
        {
            int count;
            var vk = new VkApi { AccessToken = "token" };
            vk.Users.Search("", out count);
        }

        [Test]
        public void Search_BadQuery_EmptyList()
        {
            const string url = "https://api.vk.com/method/users.search?q=fa'sosjvsoidf&count=20&access_token=token";

            const string json =
                @"{
                    'response': [
                      0
                    ]
                  }";

            int count;
            var users = GetMockedUsersCategory(url, json);
            var lst = users.Search("fa'sosjvsoidf", out count).ToList();

            Assert.That(count, Is.EqualTo(0));
            Assert.That(lst, Is.Not.Null);
            Assert.That(lst.Count, Is.EqualTo(0));
        }

        [Test]
        public void Search_EducationField_ListofProfileObjects()
        {
            const string url = "https://api.vk.com/method/users.search?q=Masha Ivanova&fields=education&count=3&offset=123&access_token=token";

            const string json =
                @"{
                    'response': [
                      26953,
                      {
                        'uid': 165614770,
                        'first_name': 'Маша',
                        'last_name': 'Иванова',
                        'university': '0',
                        'university_name': '',
                        'faculty': '0',
                        'faculty_name': '',
                        'graduation': '0'
                      },
                      {
                        'uid': 174063570,
                        'first_name': 'Маша',
                        'last_name': 'Иванова',
                        'university': '0',
                        'university_name': '',
                        'faculty': '0',
                        'faculty_name': '',
                        'graduation': '0'
                      },
                      {
                        'uid': 76817368,
                        'first_name': 'Маша',
                        'last_name': 'Иванова',
                        'university': '0',
                        'university_name': '',
                        'faculty': '0',
                        'faculty_name': '',
                        'graduation': '0'
                      }
                    ]
                  }";

            int count;
            var users = GetMockedUsersCategory(url, json);
            var lst = users.Search(Query, out count, ProfileFields.Education, 3, 123).ToList();

            Assert.That(count, Is.EqualTo(26953));
            Assert.That(lst.Count, Is.EqualTo(3));
            Assert.That(lst[0], Is.Not.Null);
            Assert.That(lst[0].Id, Is.EqualTo(165614770));
            Assert.That(lst[0].FirstName, Is.EqualTo("Маша"));
            Assert.That(lst[0].LastName, Is.EqualTo("Иванова"));
            Assert.That(lst[0].Education, Is.Null);

            Assert.That(lst[1], Is.Not.Null);
            Assert.That(lst[1].Id, Is.EqualTo(174063570));
            Assert.That(lst[1].FirstName, Is.EqualTo("Маша"));
            Assert.That(lst[1].LastName, Is.EqualTo("Иванова"));
            Assert.That(lst[1].Education, Is.Null);

            Assert.That(lst[2], Is.Not.Null);
            Assert.That(lst[2].Id, Is.EqualTo(76817368));
            Assert.That(lst[2].FirstName, Is.EqualTo("Маша"));
            Assert.That(lst[2].LastName, Is.EqualTo("Иванова"));
            Assert.That(lst[2].Education, Is.Null);
        }

        [Test]
        public void Search_DefaultFields_ListOfProfileObjects()
        {
            const string url = "https://api.vk.com/method/users.search?q=Masha Ivanova&count=20&access_token=token";

            const string json =
                @"{
                    'response': [
                      26953,
                      {
                        'uid': 449928,
                        'first_name': 'Маша',
                        'last_name': 'Иванова'
                      },
                      {
                        'uid': 70145254,
                        'first_name': 'Маша',
                        'last_name': 'Шаблинская-Иванова'
                      },
                      {
                        'uid': 62899425,
                        'first_name': 'Masha',
                        'last_name': 'Ivanova'
                      }
                    ]
                  }";

            int count;
            var users = GetMockedUsersCategory(url, json);
            var lst = users.Search(Query, out count).ToList();

            Assert.That(count, Is.EqualTo(26953));
            Assert.That(lst.Count, Is.EqualTo(3));
            Assert.That(lst[0], Is.Not.Null);
            Assert.That(lst[0].Id, Is.EqualTo(449928));
            Assert.That(lst[0].FirstName, Is.EqualTo("Маша"));
            Assert.That(lst[0].LastName, Is.EqualTo("Иванова"));

            Assert.That(lst[1], Is.Not.Null);
            Assert.That(lst[1].Id, Is.EqualTo(70145254));
            Assert.That(lst[1].FirstName, Is.EqualTo("Маша"));
            Assert.That(lst[1].LastName, Is.EqualTo("Шаблинская-Иванова"));

            Assert.That(lst[2], Is.Not.Null);
            Assert.That(lst[2].Id, Is.EqualTo(62899425));
            Assert.That(lst[2].FirstName, Is.EqualTo("Masha"));
            Assert.That(lst[2].LastName, Is.EqualTo("Ivanova"));
        }
        
        // ===================================================================
        [Test]
        public void IsAppUser_5_5_version_of_api_return_false()
        {
            const string url = "https://api.vk.com/method/users.isAppUser?user_id=1&v=5.9&access_token=token";
            const string json =
                @"{
                    'response': 0
                  }";

            UsersCategory cat = GetMockedUsersCategory(url, json);

            bool result = cat.IsAppUser(1);

            result.ShouldBeFalse();
        }

        [Test]
        public void IsAppUser_5_5_version_of_api_return_true()
        {
            const string url = "https://api.vk.com/method/users.isAppUser?user_id=123&v=5.9&access_token=token";
            const string json =
                @"{
                    'response': 1
                  }";

            UsersCategory cat = GetMockedUsersCategory(url, json);

            bool result = cat.IsAppUser(123);

            result.ShouldBeTrue();
        }

        [Test]
        public void Get_ListOfUsers()
        {
            const string url = "https://api.vk.com/method/users.get?fields=uid,first_name,last_name,sex,bdate,city,country,photo_50,photo_100,photo_200,photo_200_orig,photo_400_orig,photo_max,photo_max_orig,online,lists,domain,has_mobile,contacts,connections,site,education,universities,schools,can_post,can_see_all_posts,can_see_audio,can_write_private_message,status,last_seen,common_count,relation,relatives,counters,nickname,timezone&name_case=gen&v=5.9&user_ids=1&access_token=token";
            const string json =
            @"{
                    'response': [
                      {
                        'id': 1,
                        'first_name': 'Павла',
                        'last_name': 'Дурова',
                        'sex': 2,
                        'nickname': '',
                        'domain': 'durov',
                        'bdate': '10.10.1984',
                        'city': {
                          'id': 2,
                          'title': 'Санкт-Петербург'
                        },
                        'country': {
                          'id': 1,
                          'title': 'Россия'
                        },
                        'timezone': 3,
                        'photo_50': 'http://cs7004.vk.me/c7003/v7003079/374b/53lwetwOxD8.jpg',
                        'photo_100': 'http://cs7004.vk.me/c7003/v7003563/359e/Hei0g6eeaAc.jpg',
                        'photo_200': 'http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg',
                        'photo_max': 'http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg',
                        'photo_200_orig': 'http://cs7004.vk.me/c7003/v7003736/3a08/mEqSflTauxA.jpg',
                        'photo_400_orig': 'http://cs7004.vk.me/c7003/v7003397/3824/JjPJbkvJxpM.jpg',
                        'photo_max_orig': 'http://cs7004.vk.me/c7003/v7003397/3824/JjPJbkvJxpM.jpg',
                        'has_mobile': 1,
                        'online': 1,
                        'can_post': 0,
                        'can_see_all_posts': 0,
                        'can_see_audio': 0,
                        'can_write_private_message': 0,
                        'twitter': 'durov',
                        'site': '',
                        'status': '',
                        'last_seen': {
                          'time': 1392634257,
                          'platform': 7
                        },
                        'common_count': 0,
                        'counters': {
                          'albums': 1,
                          'videos': 8,
                          'audios': 0,
                          'notes': 6,
                          'photos': 153,
                          'friends': 688,
                          'online_friends': 146,
                          'mutual_friends': 0,
                          'followers': 5934786,
                          'subscriptions': 0,
                          'pages': 51
                        },
                        'university': 1,
                        'university_name': '',
                        'faculty': 0,
                        'faculty_name': '',
                        'graduation': 2006,
                        'relation': 0,
                        'universities': [
                          {
                            'id': 1,
                            'country': 1,
                            'city': 2,
                            'name': 'СПбГУ',
                            'graduation': 2006
                          }
                        ],
                        'schools': [
                          {
                            'id': '1035386',
                            'country': '88',
                            'city': '16',
                            'name': 'Sc.Elem. Coppino - Falletti di Barolo',
                            'year_from': 1990,
                            'year_to': 1992,
                            'class': ''
                          },
                          {
                            'id': '1',
                            'country': '1',
                            'city': '2',
                            'name': 'Академическая (АГ) СПбГУ',
                            'year_from': 1996,
                            'year_to': 2001,
                            'year_graduated': 2001,
                            'class': 'о',
                            'type': 1,
                            'type_str': 'Гимназия'
                          }
                        ],
                        'relatives': []
                      }
                    ]
                  }";

            UsersCategory cat = GetMockedUsersCategory(url, json);

            ReadOnlyCollection<User> result = cat.Get(new long[] {1}, ProfileFields.All, NameCase.Gen);

            result.ShouldNotBeNull();
            result.Count.ShouldEqual(1);
            
            var u = result[0];
            u.Id.ShouldEqual(1);
            u.FirstName.ShouldEqual("Павла");
            u.LastName.ShouldEqual("Дурова");
            u.Sex.ShouldEqual(Sex.Male);
            u.Nickname.ShouldEqual(string.Empty);
            u.Domain.ShouldEqual("durov");
            u.BirthDate.ShouldEqual("10.10.1984");
            u.City.ShouldNotBeNull();
            u.City.Id.ShouldEqual(2);
            u.City.Title.ShouldEqual("Санкт-Петербург");
            u.Country.ShouldNotBeNull();
            u.Country.Id.ShouldEqual(1);
            u.Country.Title.ShouldEqual("Россия");
            u.Timezone.ShouldEqual(3);
            u.PhotoPreviews.Photo50.ShouldEqual(new Uri("http://cs7004.vk.me/c7003/v7003079/374b/53lwetwOxD8.jpg"));
            u.PhotoPreviews.Photo100.ShouldEqual(new Uri("http://cs7004.vk.me/c7003/v7003563/359e/Hei0g6eeaAc.jpg"));
            u.PhotoPreviews.Photo200.ShouldEqual(new Uri("http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg"));
            u.PhotoPreviews.Photo400.ShouldEqual("http://cs7004.vk.me/c7003/v7003397/3824/JjPJbkvJxpM.jpg");
            u.PhotoPreviews.PhotoMax.ShouldEqual("http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg");
            u.HasMobile.HasValue.ShouldBeTrue();
            u.HasMobile.Value.ShouldBeTrue();
            u.Online.HasValue.ShouldBeTrue();
            u.Online.Value.ShouldBeTrue();
            u.CanPost.ShouldBeFalse();
            u.CanSeeAllPosts.ShouldBeFalse();
            u.CanSeeAudio.ShouldBeFalse();
            u.CanWritePrivateMessage.ShouldBeFalse();
            u.Connections.Twitter.ShouldEqual("durov");
            u.Site.ShouldEqual(string.Empty);
            u.Status.ShouldEqual(string.Empty);
            // TODO: u.LastSeen
            u.CommonCount.Value.ShouldEqual(0);
            u.Counters.Albums.ShouldEqual(1);
            u.Counters.Videos.ShouldEqual(8);
            u.Counters.Audios.ShouldEqual(0);
            u.Counters.Notes.Value.ShouldEqual(6);
            u.Counters.Photos.Value.ShouldEqual(153);
            u.Counters.Friends.Value.ShouldEqual(688);
            u.Counters.OnlineFriends.ShouldEqual(146);
            u.Counters.MutualFriends.ShouldEqual(0);
            u.Counters.Followers.ShouldEqual(5934786);
            u.Counters.Subscriptions.ShouldEqual(0);
            u.Counters.Pages.ShouldEqual(51);
            u.Universities.Count.ShouldEqual(1);
            u.Universities[0].Id.ShouldEqual(1);
            u.Universities[0].Country.ShouldEqual(1);
            u.Universities[0].City.ShouldEqual(2);
            u.Universities[0].Name.ShouldEqual("СПбГУ");
            u.Universities[0].Graduation.ShouldEqual(2006);

            u.Schools.Count.ShouldEqual(2);
            u.Schools[0].Id.ShouldEqual(1035386);
            u.Schools[0].Country.ShouldEqual(88);
            u.Schools[0].City.ShouldEqual(16);
            u.Schools[0].Name.ShouldEqual("Sc.Elem. Coppino - Falletti di Barolo");
            u.Schools[0].YearFrom.ShouldEqual(1990);
            u.Schools[0].YearTo.ShouldEqual(1992);
            u.Schools[0].Class.ShouldEqual(string.Empty);

            u.Schools[1].Id.ShouldEqual(1);
            u.Schools[1].Country.ShouldEqual(1);
            u.Schools[1].City.ShouldEqual(2);
            u.Schools[1].Name.ShouldEqual("Академическая (АГ) СПбГУ");
            u.Schools[1].YearFrom.ShouldEqual(1996);
            u.Schools[1].YearTo.ShouldEqual(2001);
            u.Schools[1].YearGraduated.ShouldEqual(2001);
            u.Schools[1].Class.ShouldEqual("о");
            u.Schools[1].Type.ShouldEqual(1);
            u.Schools[1].TypeStr.ShouldEqual("Гимназия");

            u.Relatives.Count.ShouldEqual(0);
        }

        [Test]
        public void Get_SingleUser()
        {
            const string url = "https://api.vk.com/method/users.get?fields=uid,first_name,last_name,sex,bdate,city,country,photo_50,photo_100,photo_200,photo_200_orig,photo_400_orig,photo_max,photo_max_orig,online,lists,domain,has_mobile,contacts,connections,site,education,universities,schools,can_post,can_see_all_posts,can_see_audio,can_write_private_message,status,last_seen,common_count,relation,relatives,counters,nickname,timezone&name_case=gen&v=5.9&user_ids=1&access_token=token";
            const string json =
            @"{
                    'response': [
                      {
                        'id': 1,
                        'first_name': 'Павла',
                        'last_name': 'Дурова',
                        'sex': 2,
                        'nickname': '',
                        'domain': 'durov',
                        'bdate': '10.10.1984',
                        'city': {
                          'id': 2,
                          'title': 'Санкт-Петербург'
                        },
                        'country': {
                          'id': 1,
                          'title': 'Россия'
                        },
                        'timezone': 3,
                        'photo_50': 'http://cs7004.vk.me/c7003/v7003079/374b/53lwetwOxD8.jpg',
                        'photo_100': 'http://cs7004.vk.me/c7003/v7003563/359e/Hei0g6eeaAc.jpg',
                        'photo_200': 'http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg',
                        'photo_max': 'http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg',
                        'photo_200_orig': 'http://cs7004.vk.me/c7003/v7003736/3a08/mEqSflTauxA.jpg',
                        'photo_400_orig': 'http://cs7004.vk.me/c7003/v7003397/3824/JjPJbkvJxpM.jpg',
                        'photo_max_orig': 'http://cs7004.vk.me/c7003/v7003397/3824/JjPJbkvJxpM.jpg',
                        'has_mobile': 1,
                        'online': 1,
                        'can_post': 0,
                        'can_see_all_posts': 0,
                        'can_see_audio': 0,
                        'can_write_private_message': 0,
                        'twitter': 'durov',
                        'site': '',
                        'status': '',
                        'last_seen': {
                          'time': 1392634257,
                          'platform': 7
                        },
                        'common_count': 0,
                        'counters': {
                          'albums': 1,
                          'videos': 8,
                          'audios': 0,
                          'notes': 6,
                          'photos': 153,
                          'friends': 688,
                          'online_friends': 146,
                          'mutual_friends': 0,
                          'followers': 5934786,
                          'subscriptions': 0,
                          'pages': 51
                        },
                        'university': 1,
                        'university_name': '',
                        'faculty': 0,
                        'faculty_name': '',
                        'graduation': 2006,
                        'relation': 0,
                        'universities': [
                          {
                            'id': 1,
                            'country': 1,
                            'city': 2,
                            'name': 'СПбГУ',
                            'graduation': 2006
                          }
                        ],
                        'schools': [
                          {
                            'id': '1035386',
                            'country': '88',
                            'city': '16',
                            'name': 'Sc.Elem. Coppino - Falletti di Barolo',
                            'year_from': 1990,
                            'year_to': 1992,
                            'class': ''
                          },
                          {
                            'id': '1',
                            'country': '1',
                            'city': '2',
                            'name': 'Академическая (АГ) СПбГУ',
                            'year_from': 1996,
                            'year_to': 2001,
                            'year_graduated': 2001,
                            'class': 'о',
                            'type': 1,
                            'type_str': 'Гимназия'
                          }
                        ],
                        'relatives': []
                      }
                    ]
                  }";

            UsersCategory cat = GetMockedUsersCategory(url, json);

            User u = cat.Get(1, ProfileFields.All, NameCase.Gen);

            u.ShouldNotBeNull();
            u.Id.ShouldEqual(1);
            u.FirstName.ShouldEqual("Павла");
            u.LastName.ShouldEqual("Дурова");
            u.Sex.ShouldEqual(Sex.Male);
            u.Nickname.ShouldEqual(string.Empty);
            u.Domain.ShouldEqual("durov");
            u.BirthDate.ShouldEqual("10.10.1984");
            u.City.ShouldNotBeNull();
            u.City.Id.ShouldEqual(2);
            u.City.Title.ShouldEqual("Санкт-Петербург");
            u.Country.ShouldNotBeNull();
            u.Country.Id.ShouldEqual(1);
            u.Country.Title.ShouldEqual("Россия");
            u.Timezone.ShouldEqual(3);
            u.PhotoPreviews.Photo50.ShouldEqual(new Uri("http://cs7004.vk.me/c7003/v7003079/374b/53lwetwOxD8.jpg"));
            u.PhotoPreviews.Photo100.ShouldEqual(new Uri("http://cs7004.vk.me/c7003/v7003563/359e/Hei0g6eeaAc.jpg"));
            u.PhotoPreviews.Photo200.ShouldEqual(new Uri("http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg"));
            u.PhotoPreviews.Photo400.ShouldEqual("http://cs7004.vk.me/c7003/v7003397/3824/JjPJbkvJxpM.jpg");
            u.PhotoPreviews.PhotoMax.ShouldEqual("http://cs7004.vk.me/c7003/v7003237/369a/x4RqtBxY4kc.jpg");
            u.HasMobile.HasValue.ShouldBeTrue();
            u.HasMobile.Value.ShouldBeTrue();
            u.Online.HasValue.ShouldBeTrue();
            u.Online.Value.ShouldBeTrue();
            u.CanPost.ShouldBeFalse();
            u.CanSeeAllPosts.ShouldBeFalse();
            u.CanSeeAudio.ShouldBeFalse();
            u.CanWritePrivateMessage.ShouldBeFalse();
            u.Connections.Twitter.ShouldEqual("durov");
            u.Site.ShouldEqual(string.Empty);
            u.Status.ShouldEqual(string.Empty);
            // TODO: u.LastSeen
            u.CommonCount.Value.ShouldEqual(0);
            u.Counters.Albums.ShouldEqual(1);
            u.Counters.Videos.ShouldEqual(8);
            u.Counters.Audios.ShouldEqual(0);
            u.Counters.Notes.Value.ShouldEqual(6);
            u.Counters.Photos.Value.ShouldEqual(153);
            u.Counters.Friends.Value.ShouldEqual(688);
            u.Counters.OnlineFriends.ShouldEqual(146);
            u.Counters.MutualFriends.ShouldEqual(0);
            u.Counters.Followers.ShouldEqual(5934786);
            u.Counters.Subscriptions.ShouldEqual(0);
            u.Counters.Pages.ShouldEqual(51);
            u.Universities.Count.ShouldEqual(1);
            u.Universities[0].Id.ShouldEqual(1);
            u.Universities[0].Country.ShouldEqual(1);
            u.Universities[0].City.ShouldEqual(2);
            u.Universities[0].Name.ShouldEqual("СПбГУ");
            u.Universities[0].Graduation.ShouldEqual(2006);

            u.Schools.Count.ShouldEqual(2);
            u.Schools[0].Id.ShouldEqual(1035386);
            u.Schools[0].Country.ShouldEqual(88);
            u.Schools[0].City.ShouldEqual(16);
            u.Schools[0].Name.ShouldEqual("Sc.Elem. Coppino - Falletti di Barolo");
            u.Schools[0].YearFrom.ShouldEqual(1990);
            u.Schools[0].YearTo.ShouldEqual(1992);
            u.Schools[0].Class.ShouldEqual(string.Empty);

            u.Schools[1].Id.ShouldEqual(1);
            u.Schools[1].Country.ShouldEqual(1);
            u.Schools[1].City.ShouldEqual(2);
            u.Schools[1].Name.ShouldEqual("Академическая (АГ) СПбГУ");
            u.Schools[1].YearFrom.ShouldEqual(1996);
            u.Schools[1].YearTo.ShouldEqual(2001);
            u.Schools[1].YearGraduated.ShouldEqual(2001);
            u.Schools[1].Class.ShouldEqual("о");
            u.Schools[1].Type.ShouldEqual(1);
            u.Schools[1].TypeStr.ShouldEqual("Гимназия");

            u.Relatives.Count.ShouldEqual(0);
        }

        [Test]
        public void Get_DeletedUser()
        {
            const string url = "https://api.vk.com/method/users.get?fields=first_name,last_name,education&v=5.9&user_ids=4793858&access_token=token";
            const string json =
                @"{
                    'response': [
                      {
                        'id': 4793858,
                        'first_name': 'Антон',
                        'last_name': 'Жидков',
                        'deactivated': 'deleted'
                      }
                    ]
                  }";

            UsersCategory cat = GetMockedUsersCategory(url, json);

            User user = cat.Get(4793858, ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Education);

            user.ShouldNotBeNull();
            user.Id.ShouldEqual(4793858);
            user.FirstName.ShouldEqual("Антон");
            user.LastName.ShouldEqual("Жидков");
            user.DeactiveReason.ShouldEqual("deleted");
            user.IsDeactivated.ShouldBeTrue();
        }
    }
}